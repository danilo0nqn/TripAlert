using TripAlert.Core.Models;

namespace TripAlert.Core.Services.FlightApis;

/// <summary>
/// Contrato que deben implementar todas las integraciones con APIs de vuelos.
/// </summary>
public interface IFlightSearchService
{
    /// <summary>
    /// Nombre de la fuente consumida.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Ejecuta una búsqueda y retorna los viajes encontrados.
    /// </summary>
    Task<IReadOnlyList<Trip>> SearchTripsAsync(UserSearchRequest request, CancellationToken cancellationToken);
}
