using TripAlert.Core.Models;

namespace TripAlert.Core.Services.FlightApis;

/// <summary>
/// Implementación simulada inspirada en Google Flights que solo soporta rutas populares.
/// </summary>
public sealed class GoogleFlightsSearchService : SimulatedFlightSearchServiceBase
{
    private static readonly HashSet<string> SupportedCityPairs = new(StringComparer.OrdinalIgnoreCase)
    {
        "Neuquén|Buenos Aires",
        "Buenos Aires|Madrid",
        "Buenos Aires|Barcelona",
        "Buenos Aires|Santiago",
        "Buenos Aires|São Paulo",
        "Buenos Aires|Miami",
        "Madrid|Barcelona",
        "Madrid|Roma"
    };

    /// <summary>
    /// Crea la instancia.
    /// </summary>
    public GoogleFlightsSearchService()
        : base("GoogleFlights", TimeSpan.FromHours(2.6), 150m)
    {
    }

    /// <inheritdoc />
    protected override string AirlineName => "Google Flights Aggregator";

    /// <inheritdoc />
    protected override decimal PriceFactor => 0.88m;

    /// <inheritdoc />
    protected override bool IsTripSupported(CityAirport origin, CityAirport destination)
    {
        var key = $"{origin.City}|{destination.City}";
        return SupportedCityPairs.Contains(key);
    }
}
