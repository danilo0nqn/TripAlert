namespace TripAlert.Core.Models;

/// <summary>
/// Representa la combinación de ciudad, país y aeropuerto para facilitar la identificación.
/// </summary>
public sealed class CityAirport
{
    /// <summary>
    /// Crea una nueva instancia.
    /// </summary>
    /// <param name="country">Nombre del país.</param>
    /// <param name="city">Nombre de la ciudad.</param>
    /// <param name="airportCode">Código IATA del aeropuerto.</param>
    /// <param name="airportName">Nombre descriptivo del aeropuerto.</param>
    public CityAirport(string country, string city, string airportCode, string airportName)
    {
        Country = country;
        City = city;
        AirportCode = airportCode;
        AirportName = airportName;
    }

    /// <summary>
    /// Nombre del país donde se ubica el aeropuerto.
    /// </summary>
    public string Country { get; }

    /// <summary>
    /// Ciudad asociada al aeropuerto.
    /// </summary>
    public string City { get; }

    /// <summary>
    /// Código IATA del aeropuerto.
    /// </summary>
    public string AirportCode { get; }

    /// <summary>
    /// Nombre descriptivo del aeropuerto.
    /// </summary>
    public string AirportName { get; }

    /// <inheritdoc />
    public override string ToString() => $"{City}, {Country} ({AirportCode})";
}
