using TripAlert.Core.Models;

namespace TripAlert.Core.Utils;

/// <summary>
/// Comparador estructural de viajes para evitar duplicados.
/// </summary>
public sealed class TripEqualityComparer : IEqualityComparer<Trip>
{
    /// <inheritdoc />
    public bool Equals(Trip? x, Trip? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        if (!string.Equals(x.DepartureAirport.AirportCode, y.DepartureAirport.AirportCode, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(x.ArrivalAirport.AirportCode, y.ArrivalAirport.AirportCode, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(x.Currency, y.Currency, StringComparison.OrdinalIgnoreCase) ||
            x.Flights.Count != y.Flights.Count)
        {
            return false;
        }

        for (var i = 0; i < x.Flights.Count; i++)
        {
            var xf = x.Flights[i];
            var yf = y.Flights[i];
            if (!string.Equals(xf.Airline, yf.Airline, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(xf.DepartureAirport.AirportCode, yf.DepartureAirport.AirportCode, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(xf.ArrivalAirport.AirportCode, yf.ArrivalAirport.AirportCode, StringComparison.OrdinalIgnoreCase) ||
                xf.DepartureTime != yf.DepartureTime ||
                xf.ArrivalTime != yf.ArrivalTime ||
                !string.Equals(xf.Currency, yf.Currency, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public int GetHashCode(Trip obj)
    {
        var hash = new HashCode();
        hash.Add(obj.DepartureAirport.AirportCode, StringComparer.OrdinalIgnoreCase);
        hash.Add(obj.ArrivalAirport.AirportCode, StringComparer.OrdinalIgnoreCase);
        hash.Add(obj.Currency, StringComparer.OrdinalIgnoreCase);
        foreach (var flight in obj.Flights)
        {
            hash.Add(flight.Airline, StringComparer.OrdinalIgnoreCase);
            hash.Add(flight.DepartureAirport.AirportCode, StringComparer.OrdinalIgnoreCase);
            hash.Add(flight.ArrivalAirport.AirportCode, StringComparer.OrdinalIgnoreCase);
            hash.Add(flight.DepartureTime);
            hash.Add(flight.ArrivalTime);
            hash.Add(flight.Currency, StringComparer.OrdinalIgnoreCase);
        }

        return hash.ToHashCode();
    }
}
