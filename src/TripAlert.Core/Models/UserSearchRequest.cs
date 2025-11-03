namespace TripAlert.Core.Models;

/// <summary>
/// Datos de entrada proporcionados por la persona usuaria para una búsqueda.
/// </summary>
public sealed class UserSearchRequest
{
    /// <summary>
    /// Ciudad de origen.
    /// </summary>
    public string OriginCity { get; init; } = string.Empty;

    /// <summary>
    /// Ciudad de destino.
    /// </summary>
    public string DestinationCity { get; init; } = string.Empty;

    /// <summary>
    /// Fecha mínima de salida.
    /// </summary>
    public DateTime DepartureFrom { get; init; }

    /// <summary>
    /// Fecha máxima de salida.
    /// </summary>
    public DateTime DepartureTo { get; init; }

    /// <summary>
    /// Prioridad seleccionada para ordenar resultados.
    /// </summary>
    public TripPriority Priority { get; init; }

    /// <summary>
    /// Moneda deseada para los resultados. Por defecto, USD para cumplir con el requerimiento.
    /// </summary>
    public string Currency { get; init; } = "USD";

    /// <summary>
    /// Máximo de escalas permitidas (por defecto, 3 según el requerimiento).
    /// </summary>
    public int MaxStops { get; init; } = 3;

    /// <summary>
    /// Cantidad de resultados destacados que se guardarán.
    /// </summary>
    public int MaxTopResults { get; init; } = 10;
}
