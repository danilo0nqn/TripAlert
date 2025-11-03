namespace TripAlert.Core.Models;

/// <summary>
/// Representa una escala entre vuelos.
/// </summary>
public sealed class Layover
{
    /// <summary>
    /// Crea una escala.
    /// </summary>
    public Layover(CityAirport airport, TimeSpan waitingTime)
    {
        Airport = airport;
        WaitingTime = waitingTime;
    }

    /// <summary>
    /// Aeropuerto donde se realiza la escala.
    /// </summary>
    public CityAirport Airport { get; }

    /// <summary>
    /// Tiempo de espera hasta el siguiente vuelo.
    /// </summary>
    public TimeSpan WaitingTime { get; }
}
