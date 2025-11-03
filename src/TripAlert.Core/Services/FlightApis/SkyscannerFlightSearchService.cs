using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TripAlert.Core.Models;

namespace TripAlert.Core.Services.FlightApis;

/// <summary>
/// Implementación que consume la API pública de Skyscanner (vía RapidAPI u otra pasarela compatible).
/// En caso de no contar con credenciales válidas, vuelve al modo simulado previo para no interrumpir la ejecución.
/// </summary>
public sealed class SkyscannerFlightSearchService : IFlightSearchService, IDisposable
{
    private const string DefaultBaseUrl = "https://skyscanner44.p.rapidapi.com/search";
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;
    private readonly string _apiKey;
    private readonly string _apiHost;
    private readonly string _baseUrl;
    private readonly string _market;
    private readonly string _locale;
    private readonly string _currency = "USD";
    private readonly IFlightSearchService _fallbackService = new SkyscannerSimulatedFallbackService();

    /// <summary>
    /// Crea la instancia.
    /// </summary>
    /// <param name="httpClient">Cliente HTTP opcional para facilitar pruebas.</param>
    /// <param name="apiKey">Token de autenticación para la API.</param>
    /// <param name="apiHost">Cabecera requerida por proveedores como RapidAPI.</param>
    /// <param name="baseUrl">URL base del endpoint de Skyscanner.</param>
    /// <param name="market">Código de mercado a consultar.</param>
    /// <param name="locale">Configuración regional deseada.</param>
    public SkyscannerFlightSearchService(
        HttpClient? httpClient = null,
        string? apiKey = null,
        string? apiHost = null,
        string? baseUrl = null,
        string? market = null,
        string? locale = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _disposeHttpClient = httpClient is null;
        _apiKey = apiKey ?? Environment.GetEnvironmentVariable("TRIPALERT_SKYSCANNER_API_KEY") ?? string.Empty;
        _apiHost = apiHost ?? Environment.GetEnvironmentVariable("TRIPALERT_SKYSCANNER_API_HOST") ?? "skyscanner44.p.rapidapi.com";
        _baseUrl = baseUrl ?? Environment.GetEnvironmentVariable("TRIPALERT_SKYSCANNER_BASE_URL") ?? DefaultBaseUrl;
        _market = market ?? Environment.GetEnvironmentVariable("TRIPALERT_SKYSCANNER_MARKET") ?? "AR";
        _locale = locale ?? Environment.GetEnvironmentVariable("TRIPALERT_SKYSCANNER_LOCALE") ?? "es-AR";
    }

    /// <inheritdoc />
    public string ProviderName => "Skyscanner";

    /// <inheritdoc />
    public async Task<IReadOnlyList<Trip>> SearchTripsAsync(UserSearchRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return await _fallbackService.SearchTripsAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var trips = new List<Trip>();
        var originAirports = CityAirportCatalog.FindByCity(request.OriginCity).ToList();
        var destinationAirports = CityAirportCatalog.FindByCity(request.DestinationCity).ToList();

        if (!originAirports.Any() || !destinationAirports.Any())
        {
            return trips;
        }

        foreach (var origin in originAirports)
        {
            foreach (var destination in destinationAirports)
            {
                foreach (var departureDate in EnumerateDates(request.DepartureFrom, request.DepartureTo))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var uri = BuildRequestUri(origin.AirportCode, destination.AirportCode, departureDate);
                    using var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
                    httpRequest.Headers.TryAddWithoutValidation("X-RapidAPI-Key", _apiKey);
                    httpRequest.Headers.TryAddWithoutValidation("X-RapidAPI-Host", _apiHost);

                    using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        continue;
                    }

                    await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                    using var document = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken).ConfigureAwait(false);
                    var parsedTrips = ParseTrips(document.RootElement, request);
                    trips.AddRange(parsedTrips);
                }
            }
        }

        if (!trips.Any())
        {
            return await _fallbackService.SearchTripsAsync(request, cancellationToken).ConfigureAwait(false);
        }

        return trips;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private Uri BuildRequestUri(string originCode, string destinationCode, DateTime departureDate)
    {
        var builder = new UriBuilder(_baseUrl);
        var queryParams = new Dictionary<string, string>
        {
            ["adults"] = "1",
            ["origin"] = originCode,
            ["destination"] = destinationCode,
            ["departureDate"] = departureDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["currency"] = _currency,
            ["market"] = _market,
            ["locale"] = _locale
        };

        builder.Query = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
        return builder.Uri;
    }

    private IReadOnlyList<Trip> ParseTrips(JsonElement root, UserSearchRequest request)
    {
        var trips = new List<Trip>();
        var legsLookup = BuildLookup(root, "legs");
        if (!TryGetArray(root, "itineraries", out var itineraries) &&
            !TryGetArray(root, new[] { "data", "itineraries" }, out itineraries))
        {
            return trips;
        }

        foreach (var itinerary in itineraries.EnumerateArray())
        {
            var legs = ExtractLegs(itinerary, legsLookup);
            if (legs.Count == 0)
            {
                continue;
            }

            var totalPrice = ExtractPrice(itinerary);
            var flights = BuildFlightsFromLegs(legs, totalPrice, request.Currency);
            if (flights.Count == 0)
            {
                continue;
            }

            var layovers = BuildLayovers(flights);
            var tripPrice = totalPrice > 0 ? totalPrice : flights.Sum(f => f.Price);
            trips.Add(new Trip(
                flights,
                layovers,
                flights.First().DepartureTime,
                flights.Last().ArrivalTime,
                tripPrice,
                flights.First().DepartureAirport,
                flights.Last().ArrivalAirport,
                request.Currency));
        }

        return trips;
    }

    private static Dictionary<string, JsonElement> BuildLookup(JsonElement root, string propertyName)
    {
        var lookup = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        if (!root.TryGetProperty(propertyName, out var collection) || collection.ValueKind != JsonValueKind.Array)
        {
            return lookup;
        }

        foreach (var item in collection.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (item.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String)
            {
                var id = idElement.GetString();
                if (!string.IsNullOrWhiteSpace(id))
                {
                    lookup[id] = item;
                }
            }
        }

        return lookup;
    }

    private static bool TryGetArray(JsonElement root, string propertyName, out JsonElement array)
    {
        if (root.TryGetProperty(propertyName, out var candidate) && candidate.ValueKind == JsonValueKind.Array)
        {
            array = candidate;
            return true;
        }

        array = default;
        return false;
    }

    private static bool TryGetArray(JsonElement root, IReadOnlyList<string> path, out JsonElement array)
    {
        var current = root;
        foreach (var segment in path)
        {
            if (!current.TryGetProperty(segment, out current))
            {
                array = default;
                return false;
            }
        }

        if (current.ValueKind == JsonValueKind.Array)
        {
            array = current;
            return true;
        }

        array = default;
        return false;
    }

    private static List<JsonElement> ExtractLegs(JsonElement itinerary, Dictionary<string, JsonElement> legsLookup)
    {
        var legs = new List<JsonElement>();
        if (!itinerary.TryGetProperty("legs", out var legsElement))
        {
            return legs;
        }

        foreach (var legEntry in legsElement.EnumerateArray())
        {
            JsonElement leg = legEntry;
            if (legEntry.ValueKind == JsonValueKind.String)
            {
                var legId = legEntry.GetString();
                if (!string.IsNullOrWhiteSpace(legId) && legsLookup.TryGetValue(legId, out var referencedLeg))
                {
                    leg = referencedLeg;
                }
            }

            if (leg.ValueKind == JsonValueKind.Object)
            {
                legs.Add(leg);
            }
        }

        return legs;
    }

    private List<Flight> BuildFlightsFromLegs(IReadOnlyList<JsonElement> legs, decimal totalPrice, string currency)
    {
        var flights = new List<Flight>();
        var effectivePrice = totalPrice > 0 && legs.Count > 0
            ? Math.Round(totalPrice / legs.Count, 2, MidpointRounding.AwayFromZero)
            : 0m;

        foreach (var leg in legs)
        {
            if (!TryGetDateTime(leg, "departure", out var departureTime) ||
                !TryGetDateTime(leg, "arrival", out var arrivalTime))
            {
                continue;
            }

            var duration = ExtractDuration(leg, departureTime, arrivalTime);
            var departureAirport = ResolveAirport(leg, "origin");
            var arrivalAirport = ResolveAirport(leg, "destination");
            var airline = ExtractCarrier(leg);
            var baggageIncluded = ExtractBaggageIncluded(leg, out var baggageNotes);

            var pricePerFlight = effectivePrice > 0 ? effectivePrice : 0m;
            flights.Add(new Flight(
                airline,
                departureAirport,
                arrivalAirport,
                departureTime,
                arrivalTime,
                pricePerFlight,
                duration,
                baggageIncluded,
                baggageNotes,
                currency));
        }

        return flights;
    }

    private static bool TryGetDateTime(JsonElement element, string propertyName, out DateTimeOffset dateTime)
    {
        dateTime = default;
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return false;
        }

        if (value.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            dateTime = parsed;
            return true;
        }

        return false;
    }

    private static TimeSpan ExtractDuration(JsonElement leg, DateTimeOffset departure, DateTimeOffset arrival)
    {
        if (leg.TryGetProperty("durationInMinutes", out var durationElement) && durationElement.TryGetInt32(out var minutes))
        {
            return TimeSpan.FromMinutes(minutes);
        }

        if (leg.TryGetProperty("duration", out durationElement))
        {
            if (durationElement.ValueKind == JsonValueKind.Number && durationElement.TryGetInt32(out minutes))
            {
                return TimeSpan.FromMinutes(minutes);
            }

            if (durationElement.ValueKind == JsonValueKind.String && int.TryParse(durationElement.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out minutes))
            {
                return TimeSpan.FromMinutes(minutes);
            }
        }

        return arrival - departure;
    }

    private static CityAirport ResolveAirport(JsonElement leg, string propertyName)
    {
        var code = ExtractAirportCode(leg, propertyName) ?? "UNK";
        var knownAirport = CityAirportCatalog.FindByCode(code);
        if (knownAirport is not null)
        {
            return knownAirport;
        }

        string city = code;
        string country = "Desconocido";
        string airportName = code;

        if (leg.TryGetProperty(propertyName, out var location) && location.ValueKind == JsonValueKind.Object)
        {
            if (location.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String)
            {
                airportName = nameElement.GetString() ?? airportName;
            }

            if (location.TryGetProperty("city", out var cityElement) && cityElement.ValueKind == JsonValueKind.String)
            {
                city = cityElement.GetString() ?? city;
            }

            if (location.TryGetProperty("country", out var countryElement) && countryElement.ValueKind == JsonValueKind.String)
            {
                country = countryElement.GetString() ?? country;
            }
        }

        return new CityAirport(country, city, code, airportName);
    }

    private static string? ExtractAirportCode(JsonElement leg, string propertyName)
    {
        if (!leg.TryGetProperty(propertyName, out var location))
        {
            return null;
        }

        if (location.ValueKind == JsonValueKind.String)
        {
            return location.GetString();
        }

        if (location.ValueKind == JsonValueKind.Object)
        {
            if (location.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String)
            {
                return idElement.GetString();
            }

            if (location.TryGetProperty("code", out var codeElement) && codeElement.ValueKind == JsonValueKind.String)
            {
                return codeElement.GetString();
            }

            if (location.TryGetProperty("displayCode", out var displayCode) && displayCode.ValueKind == JsonValueKind.String)
            {
                return displayCode.GetString();
            }
        }

        return null;
    }

    private static string ExtractCarrier(JsonElement leg)
    {
        if (leg.TryGetProperty("carriers", out var carriers) && carriers.ValueKind == JsonValueKind.Object)
        {
            if (carriers.TryGetProperty("marketing", out var marketing) && marketing.ValueKind == JsonValueKind.Array)
            {
                foreach (var carrier in marketing.EnumerateArray())
                {
                    if (carrier.ValueKind == JsonValueKind.Object &&
                        carrier.TryGetProperty("name", out var nameElement) &&
                        nameElement.ValueKind == JsonValueKind.String)
                    {
                        return nameElement.GetString() ?? "Skyscanner";
                    }

                    if (carrier.ValueKind == JsonValueKind.String)
                    {
                        return carrier.GetString() ?? "Skyscanner";
                    }
                }
            }
        }

        if (leg.TryGetProperty("carrierName", out var carrierName) && carrierName.ValueKind == JsonValueKind.String)
        {
            return carrierName.GetString() ?? "Skyscanner";
        }

        return "Skyscanner";
    }

    private static bool ExtractBaggageIncluded(JsonElement leg, out string notes)
    {
        if (leg.TryGetProperty("baggage", out var baggage) && baggage.ValueKind == JsonValueKind.Object)
        {
            if (baggage.TryGetProperty("included", out var included) && included.ValueKind == JsonValueKind.True)
            {
                notes = "Equipaje incluido";
                return true;
            }

            if (baggage.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
            {
                notes = text.GetString() ?? "Equipaje no informado";
                return false;
            }
        }

        notes = "Información de equipaje no disponible";
        return false;
    }

    private static List<Layover> BuildLayovers(IReadOnlyList<Flight> flights)
    {
        var layovers = new List<Layover>();
        for (var i = 1; i < flights.Count; i++)
        {
            var previousFlight = flights[i - 1];
            var currentFlight = flights[i];
            var waitingTime = currentFlight.DepartureTime - previousFlight.ArrivalTime;
            if (waitingTime < TimeSpan.Zero)
            {
                waitingTime = TimeSpan.Zero;
            }

            layovers.Add(new Layover(currentFlight.DepartureAirport, waitingTime));
        }

        return layovers;
    }

    private static decimal ExtractPrice(JsonElement itinerary)
    {
        if (itinerary.TryGetProperty("price", out var priceElement))
        {
            if (TryReadDecimal(priceElement, out var price))
            {
                return price;
            }

            if (priceElement.ValueKind == JsonValueKind.Object)
            {
                if (priceElement.TryGetProperty("raw", out var rawElement) && TryReadDecimal(rawElement, out price))
                {
                    return price;
                }

                if (priceElement.TryGetProperty("amount", out var amountElement) && TryReadDecimal(amountElement, out price))
                {
                    return price;
                }
            }
        }

        if (itinerary.TryGetProperty("pricingOptions", out var options) && options.ValueKind == JsonValueKind.Array)
        {
            foreach (var option in options.EnumerateArray())
            {
                if (option.ValueKind == JsonValueKind.Object && option.TryGetProperty("price", out var optionPrice) && TryReadDecimal(optionPrice, out var price))
                {
                    return price;
                }
            }
        }

        return 0m;
    }

    private static bool TryReadDecimal(JsonElement element, out decimal value)
    {
        value = 0m;
        switch (element.ValueKind)
        {
            case JsonValueKind.Number:
                return element.TryGetDecimal(out value);
            case JsonValueKind.String:
                var text = element.GetString();
                if (!string.IsNullOrWhiteSpace(text) && decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                {
                    value = parsed;
                    return true;
                }
                break;
        }

        return false;
    }

    private static IEnumerable<DateTime> EnumerateDates(DateTime from, DateTime to)
    {
        for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
        {
            yield return date;
        }
    }

    private sealed class SkyscannerSimulatedFallbackService : SimulatedFlightSearchServiceBase
    {
        public SkyscannerSimulatedFallbackService()
            : base("Skyscanner", TimeSpan.FromHours(2.5), 120m)
        {
        }

        /// <inheritdoc />
        protected override string AirlineName => "Skyscanner Airways";

        /// <inheritdoc />
        protected override decimal PriceFactor => 0.92m;
    }
}
