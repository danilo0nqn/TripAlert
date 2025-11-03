using System.Text.Json;
using TripAlert.Core.Models;

namespace TripAlert.Core.Services.Persistence;

/// <summary>
/// Implementación que guarda los viajes en un archivo JSON local.
/// </summary>
public sealed class JsonTripPersistenceService : ITripPersistenceService
{
    private readonly string _storagePath;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Inicializa la instancia con la ruta deseada.
    /// </summary>
    public JsonTripPersistenceService(string storagePath)
    {
        _storagePath = storagePath;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Trip>> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_storagePath))
        {
            return Array.Empty<Trip>();
        }

        await using var stream = File.OpenRead(_storagePath);
        var trips = await JsonSerializer.DeserializeAsync<List<Trip>>(stream, _serializerOptions, cancellationToken).ConfigureAwait(false);
        return trips ?? new List<Trip>();
    }

    /// <inheritdoc />
    public async Task SaveAsync(IEnumerable<Trip> trips, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_storagePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Open(_storagePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, trips.ToList(), _serializerOptions, cancellationToken).ConfigureAwait(false);
    }
}
