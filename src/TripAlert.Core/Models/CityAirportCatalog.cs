using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TripAlert.Core.Models;

/// <summary>
/// Catálogo estático con los aeropuertos más utilizados.
/// </summary>
public static class CityAirportCatalog
{
    private static readonly ReadOnlyCollection<CityAirport> _airports = new(new List<CityAirport>
    {
        new("Argentina", "Neuquén", "NQN", "Aeropuerto Internacional Presidente Perón"),
        new("Argentina", "Buenos Aires", "EZE", "Aeropuerto Internacional Ministro Pistarini (Ezeiza)"),
        new("Argentina", "Buenos Aires", "AEP", "Aeroparque Jorge Newbery"),
        new("Argentina", "Córdoba", "COR", "Aeropuerto Internacional Ingeniero Ambrosio Taravella"),
        new("Argentina", "Mendoza", "MDZ", "Aeropuerto Internacional El Plumerillo"),
        new("Argentina", "Bariloche", "BRC", "Aeropuerto Internacional Teniente Luis Candelaria"),
        new("Argentina", "Salta", "SLA", "Aeropuerto Internacional Martín Miguel de Güemes"),
        new("Brasil", "São Paulo", "GRU", "Aeroporto Internacional de Guarulhos"),
        new("Brasil", "São Paulo", "CGH", "Aeroporto de Congonhas"),
        new("Brasil", "Rio de Janeiro", "GIG", "Aeroporto Internacional do Galeão"),
        new("Brasil", "Rio de Janeiro", "SDU", "Aeroporto Santos Dumont"),
        new("Chile", "Santiago", "SCL", "Aeropuerto Internacional Arturo Merino Benítez"),
        new("Uruguay", "Montevideo", "MVD", "Aeropuerto Internacional de Carrasco"),
        new("Paraguay", "Asunción", "ASU", "Aeropuerto Internacional Silvio Pettirossi"),
        new("Perú", "Lima", "LIM", "Aeropuerto Internacional Jorge Chávez"),
        new("Estados Unidos", "Nueva York", "JFK", "John F. Kennedy International Airport"),
        new("Estados Unidos", "Nueva York", "LGA", "LaGuardia Airport"),
        new("Estados Unidos", "Miami", "MIA", "Miami International Airport"),
        new("Estados Unidos", "Los Ángeles", "LAX", "Los Angeles International Airport"),
        new("Estados Unidos", "Orlando", "MCO", "Orlando International Airport"),
        new("España", "Madrid", "MAD", "Aeropuerto Adolfo Suárez Madrid-Barajas"),
        new("España", "Barcelona", "BCN", "Aeropuerto Josep Tarradellas Barcelona-El Prat"),
        new("España", "Sevilla", "SVQ", "Aeropuerto de Sevilla"),
        new("España", "Valencia", "VLC", "Aeropuerto de Valencia"),
        new("Reino Unido", "Londres", "LHR", "Heathrow Airport"),
        new("Reino Unido", "Londres", "LGW", "Gatwick Airport"),
        new("Francia", "París", "CDG", "Aéroport Charles de Gaulle"),
        new("Francia", "París", "ORY", "Aéroport de Paris-Orly"),
        new("Italia", "Roma", "FCO", "Aeroporto di Roma-Fiumicino"),
        new("Italia", "Milán", "MXP", "Aeroporto di Milano-Malpensa"),
        new("Alemania", "Berlín", "BER", "Flughafen Berlin Brandenburg"),
        new("Alemania", "Fráncfort", "FRA", "Flughafen Frankfurt am Main"),
        new("Países Bajos", "Ámsterdam", "AMS", "Amsterdam Airport Schiphol"),
        new("Turquía", "Estambul", "IST", "İstanbul Havalimanı"),
        new("Emiratos Árabes Unidos", "Dubái", "DXB", "Dubai International Airport"),
        new("Qatar", "Doha", "DOH", "Hamad International Airport"),
        new("Australia", "Sídney", "SYD", "Sydney Kingsford Smith Airport"),
        new("Japón", "Tokio", "HND", "Haneda Airport"),
        new("Japón", "Tokio", "NRT", "Narita International Airport"),
        new("China", "Pekín", "PEK", "Beijing Capital International Airport"),
        new("China", "Shanghái", "PVG", "Shanghai Pudong International Airport"),
        new("México", "Ciudad de México", "MEX", "Aeropuerto Internacional Benito Juárez"),
        new("Colombia", "Bogotá", "BOG", "Aeropuerto Internacional El Dorado"),
        new("Panamá", "Ciudad de Panamá", "PTY", "Aeropuerto Internacional de Tocumen"),
        new("Costa Rica", "San José", "SJO", "Aeropuerto Internacional Juan Santamaría"),
        new("Canadá", "Toronto", "YYZ", "Toronto Pearson International Airport"),
        new("Canadá", "Vancouver", "YVR", "Vancouver International Airport"),
        new("Sudáfrica", "Ciudad del Cabo", "CPT", "Cape Town International Airport"),
        new("Egipto", "El Cairo", "CAI", "Cairo International Airport"),
        new("Tailandia", "Bangkok", "BKK", "Suvarnabhumi Airport"),
        new("Singapur", "Singapur", "SIN", "Singapore Changi Airport"),
        new("Nueva Zelanda", "Auckland", "AKL", "Auckland Airport"),
        new("Portugal", "Lisboa", "LIS", "Aeroporto Humberto Delgado"),
        new("Portugal", "Oporto", "OPO", "Aeroporto Francisco Sá Carneiro"),
        new("Grecia", "Atenas", "ATH", "Aeropuerto Internacional Eleftherios Venizelos"),
        new("Suiza", "Zúrich", "ZRH", "Flughafen Zürich"),
        new("Suiza", "Ginebra", "GVA", "Aéroport de Genève"),
        new("Emiratos Árabes Unidos", "Abu Dabi", "AUH", "Abu Dhabi International Airport"),
        new("India", "Delhi", "DEL", "Indira Gandhi International Airport"),
        new("India", "Mumbai", "BOM", "Chhatrapati Shivaji Maharaj International Airport"),
        new("Corea del Sur", "Seúl", "ICN", "Incheon International Airport"),
        new("Estados Unidos", "San Francisco", "SFO", "San Francisco International Airport"),
        new("Estados Unidos", "Chicago", "ORD", "O'Hare International Airport"),
        new("Estados Unidos", "Dallas", "DFW", "Dallas/Fort Worth International Airport"),
        new("Estados Unidos", "Atlanta", "ATL", "Hartsfield-Jackson Atlanta International Airport")
    });

    /// <summary>
    /// Retorna la colección de aeropuertos soportados.
    /// </summary>
    public static IReadOnlyList<CityAirport> Airports => _airports;

    /// <summary>
    /// Busca un aeropuerto por ciudad, ignorando mayúsculas/minúsculas.
    /// </summary>
    public static IEnumerable<CityAirport> FindByCity(string city) =>
        _airports.Where(a => string.Equals(a.City, city, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Obtiene un aeropuerto por código IATA.
    /// </summary>
    public static CityAirport? FindByCode(string airportCode) =>
        _airports.FirstOrDefault(a => string.Equals(a.AirportCode, airportCode, StringComparison.OrdinalIgnoreCase));
}
