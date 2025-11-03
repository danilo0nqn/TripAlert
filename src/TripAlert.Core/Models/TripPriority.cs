namespace TripAlert.Core.Models;

/// <summary>
/// Prioridad declarada por el usuario para ordenar los viajes recomendados.
/// </summary>
public enum TripPriority
{
    /// <summary>
    /// Indica que el precio total del viaje es la principal prioridad.
    /// </summary>
    Precio,

    /// <summary>
    /// Indica que la duración total del viaje es la prioridad.
    /// </summary>
    Tiempo,

    /// <summary>
    /// Indica que la cantidad de escalas es el criterio más importante.
    /// </summary>
    Escalas
}
