using TripAlert.Core.Models;

namespace TripAlert.Core.Services.FlightApis;

/// <summary>
/// Implementación base que genera resultados simulados para permitir pruebas de integración sin depender de APIs reales.
/// </summary>
public abstract class SimulatedFlightSearchServiceBase : IFlightSearchService
{
    private readonly TimeSpan _baseDuration;
    private readonly decimal _basePrice;

    /// <summary>
    /// Inicializa la instancia.
    /// </summary>
    protected SimulatedFlightSearchServiceBase(string providerName, TimeSpan baseDuration, decimal basePrice)
    {
        ProviderName = providerName;
        _baseDuration = baseDuration;
        _basePrice = basePrice;
    }

    /// <inheritdoc />
    public string ProviderName { get; }

    /// <summary>
    /// Factor multiplicador aplicado a la tarifa base para esta integración.
    /// </summary>
    protected virtual decimal PriceFactor => 1m;

    /// <summary>
    /// Ajuste adicional aplicado a la duración base.
    /// </summary>
    protected virtual TimeSpan DurationOffset => TimeSpan.Zero;

    /// <summary>
    /// Permite a las implementaciones decidir si un viaje simulado es válido.
    /// </summary>
    protected virtual bool IsTripSupported(CityAirport origin, CityAirport destination) => true;

    /// <inheritdoc />
    public Task<IReadOnlyList<Trip>> SearchTripsAsync(UserSearchRequest request, CancellationToken cancellationToken)
    {
        var results = new List<Trip>();
        var originAirports = CityAirportCatalog.FindByCity(request.OriginCity).ToList();
        var destinationAirports = CityAirportCatalog.FindByCity(request.DestinationCity).ToList();

        if (!originAirports.Any() || !destinationAirports.Any())
        {
            return Task.FromResult<IReadOnlyList<Trip>>(results);
        }

        foreach (var origin in originAirports)
        {
            foreach (var destination in destinationAirports)
            {
                if (!IsTripSupported(origin, destination))
                {
                    continue;
                }

                foreach (var departureDate in EnumerateDates(request.DepartureFrom, request.DepartureTo))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var departureTime = new DateTimeOffset(departureDate, TimeSpan.FromHours(8));
                    var duration = _baseDuration + DurationOffset;
                    var arrivalTime = departureTime + duration;
                    var baggageIncluded = ShouldIncludeBaggage(origin, destination);
                    var baggageNotes = baggageIncluded ? "Equipaje incluido" : "Equipaje despachado con costo adicional";
                    var price = Math.Round(_basePrice * PriceFactor * GetDynamicPriceModifier(departureDate), 2, MidpointRounding.AwayFromZero);

                    var flight = new Flight(
                        AirlineName,
                        origin,
                        destination,
                        departureTime,
                        arrivalTime,
                        price,
                        duration,
                        baggageIncluded,
                        baggageNotes);

                    var trip = new Trip(
                        new[] { flight },
                        Array.Empty<Layover>(),
                        departureTime,
                        arrivalTime,
                        price,
                        origin,
                        destination);

                    results.Add(trip);
                }
            }
        }

        return Task.FromResult<IReadOnlyList<Trip>>(results);
    }

    /// <summary>
    /// Nombre de la aerolínea representativa para la integración.
    /// </summary>
    protected abstract string AirlineName { get; }

    private static IEnumerable<DateTime> EnumerateDates(DateTime from, DateTime to)
    {
        for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
        {
            yield return date;
        }
    }

    private static bool ShouldIncludeBaggage(CityAirport origin, CityAirport destination)
    {
        var international = !string.Equals(origin.Country, destination.Country, StringComparison.OrdinalIgnoreCase);
        return international;
    }

    private static decimal GetDynamicPriceModifier(DateTime departureDate)
    {
        var dayOfWeekFactor = departureDate.DayOfWeek switch
        {
            DayOfWeek.Friday or DayOfWeek.Sunday => 1.25m,
            DayOfWeek.Saturday => 1.15m,
            DayOfWeek.Monday => 0.95m,
            _ => 1.0m
        };

        return dayOfWeekFactor;
    }
}
