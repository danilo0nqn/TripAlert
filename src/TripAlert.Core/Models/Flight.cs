namespace TripAlert.Core.Models;

/// <summary>
/// Representa un vuelo individual dentro de un viaje.
/// </summary>
public sealed class Flight
{
    /// <summary>
    /// Crea una nueva instancia del vuelo.
    /// </summary>
    public Flight(
        string airline,
        CityAirport departureAirport,
        CityAirport arrivalAirport,
        DateTimeOffset departureTime,
        DateTimeOffset arrivalTime,
        decimal price,
        TimeSpan duration,
        bool baggageIncluded,
        string baggageNotes)
    {
        Airline = airline;
        DepartureAirport = departureAirport;
        ArrivalAirport = arrivalAirport;
        DepartureTime = departureTime;
        ArrivalTime = arrivalTime;
        Price = price;
        Duration = duration;
        BaggageIncluded = baggageIncluded;
        BaggageNotes = baggageNotes;
    }

    /// <summary>
    /// Compañía aérea responsable del vuelo.
    /// </summary>
    public string Airline { get; }

    /// <summary>
    /// Aeropuerto de salida.
    /// </summary>
    public CityAirport DepartureAirport { get; }

    /// <summary>
    /// Aeropuerto de llegada.
    /// </summary>
    public CityAirport ArrivalAirport { get; }

    /// <summary>
    /// Fecha y hora de salida.
    /// </summary>
    public DateTimeOffset DepartureTime { get; }

    /// <summary>
    /// Fecha y hora de llegada.
    /// </summary>
    public DateTimeOffset ArrivalTime { get; }

    /// <summary>
    /// Precio individual del tramo.
    /// </summary>
    public decimal Price { get; }

    /// <summary>
    /// Duración estimada del vuelo.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Indica si el equipaje despachado está incluido.
    /// </summary>
    public bool BaggageIncluded { get; }

    /// <summary>
    /// Notas adicionales sobre políticas de equipaje.
    /// </summary>
    public string BaggageNotes { get; }
}
