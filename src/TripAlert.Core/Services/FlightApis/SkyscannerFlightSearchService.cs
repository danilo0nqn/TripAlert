namespace TripAlert.Core.Services.FlightApis;

/// <summary>
/// Implementación simulada para la API de Skyscanner.
/// </summary>
public sealed class SkyscannerFlightSearchService : SimulatedFlightSearchServiceBase
{
    /// <summary>
    /// Crea la instancia.
    /// </summary>
    public SkyscannerFlightSearchService()
        : base("Skyscanner", TimeSpan.FromHours(2.5), 120m)
    {
    }

    /// <inheritdoc />
    protected override string AirlineName => "SkyScanner Airways";

    /// <inheritdoc />
    protected override decimal PriceFactor => 0.92m;
}
