using TripAlert.Core.Models;

namespace TripAlert.Core.Services.Persistence;

/// <summary>
/// Abstracción para persistir los mejores viajes encontrados.
/// </summary>
public interface ITripPersistenceService
{
    /// <summary>
    /// Carga los viajes previamente almacenados.
    /// </summary>
    Task<IReadOnlyList<Trip>> LoadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Guarda los viajes destacados.
    /// </summary>
    Task SaveAsync(IEnumerable<Trip> trips, CancellationToken cancellationToken);
}
