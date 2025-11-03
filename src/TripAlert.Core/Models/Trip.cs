namespace TripAlert.Core.Models;

/// <summary>
/// Representa un viaje completo compuesto por uno o más vuelos.
/// </summary>
public sealed class Trip
{
    /// <summary>
    /// Crea un viaje.
    /// </summary>
    public Trip(
        IReadOnlyList<Flight> flights,
        IReadOnlyList<Layover> layovers,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        decimal totalPrice,
        CityAirport departureAirport,
        CityAirport arrivalAirport)
    {
        Flights = flights;
        Layovers = layovers;
        StartTime = startTime;
        EndTime = endTime;
        TotalPrice = totalPrice;
        DepartureAirport = departureAirport;
        ArrivalAirport = arrivalAirport;
    }

    /// <summary>
    /// Vuelos que integran el viaje.
    /// </summary>
    public IReadOnlyList<Flight> Flights { get; }

    /// <summary>
    /// Escalas intermedias.
    /// </summary>
    public IReadOnlyList<Layover> Layovers { get; }

    /// <summary>
    /// Inicio del viaje (salida del primer vuelo).
    /// </summary>
    public DateTimeOffset StartTime { get; }

    /// <summary>
    /// Fin del viaje (llegada del último vuelo).
    /// </summary>
    public DateTimeOffset EndTime { get; }

    /// <summary>
    /// Precio total acumulado del viaje.
    /// </summary>
    public decimal TotalPrice { get; }

    /// <summary>
    /// Aeropuerto del primer vuelo.
    /// </summary>
    public CityAirport DepartureAirport { get; }

    /// <summary>
    /// Aeropuerto de llegada final.
    /// </summary>
    public CityAirport ArrivalAirport { get; }

    /// <summary>
    /// Duración total considerando vuelos y escalas.
    /// </summary>
    public TimeSpan TotalDuration => EndTime - StartTime;

    /// <summary>
    /// Cantidad de escalas realizadas.
    /// </summary>
    public int Stops => Math.Max(0, Flights.Count - 1);
}
