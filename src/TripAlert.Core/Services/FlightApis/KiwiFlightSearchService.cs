namespace TripAlert.Core.Services.FlightApis;

/// <summary>
/// Implementación simulada para la API de Kiwi.
/// </summary>
public sealed class KiwiFlightSearchService : SimulatedFlightSearchServiceBase
{
    /// <summary>
    /// Crea la instancia.
    /// </summary>
    public KiwiFlightSearchService()
        : base("Kiwi", TimeSpan.FromHours(3.1), 135m)
    {
    }

    /// <inheritdoc />
    protected override string AirlineName => "Kiwi Connect";

    /// <inheritdoc />
    protected override decimal PriceFactor => 1.05m;
}
