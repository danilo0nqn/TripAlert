namespace TripAlert.Core.Services.FlightApis;

/// <summary>
/// Implementación simulada para la API de Amadeus.
/// </summary>
public sealed class AmadeusFlightSearchService : SimulatedFlightSearchServiceBase
{
    /// <summary>
    /// Crea la instancia.
    /// </summary>
    public AmadeusFlightSearchService()
        : base("Amadeus", TimeSpan.FromHours(2.8), 142m)
    {
    }

    /// <inheritdoc />
    protected override string AirlineName => "Amadeus Global";

    /// <inheritdoc />
    protected override decimal PriceFactor => 0.99m;

    /// <inheritdoc />
    protected override TimeSpan DurationOffset => TimeSpan.FromMinutes(25);
}
