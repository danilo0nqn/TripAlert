using System.Globalization;
using System.Text;
using TripAlert.Core.Models;
using TripAlert.Core.Services.FlightApis;
using TripAlert.Core.Services.Notifications;
using TripAlert.Core.Services.Persistence;
using TripAlert.Core.Services.WebScraping;
using TripAlert.Core.Utils;

namespace TripAlert.Core.Automation;

/// <summary>
/// Clase principal encargada de ejecutar el ciclo de consulta y notificación cada 24 horas.
/// </summary>
public sealed class TripAlertAutomation
{
    private readonly IReadOnlyList<IFlightSearchService> _flightServices;
    private readonly IWebScrapingService _webScrapingService;
    private readonly ITripPersistenceService _tripPersistenceService;
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IWhatsAppNotificationService _whatsAppNotificationService;
    private readonly TripEqualityComparer _tripComparer = new();
    private readonly TimeSpan _executionInterval;
    private readonly string _telegramRecipient;
    private readonly string _whatsAppRecipient;

    /// <summary>
    /// Inicializa la automatización.
    /// </summary>
    public TripAlertAutomation(
        IEnumerable<IFlightSearchService> flightServices,
        IWebScrapingService webScrapingService,
        ITripPersistenceService tripPersistenceService,
        ITelegramNotificationService telegramNotificationService,
        IWhatsAppNotificationService whatsAppNotificationService,
        TimeSpan? executionInterval = null,
        string? telegramRecipient = null,
        string? whatsAppRecipient = null)
    {
        _flightServices = flightServices.ToList();
        _webScrapingService = webScrapingService;
        _tripPersistenceService = tripPersistenceService;
        _telegramNotificationService = telegramNotificationService;
        _whatsAppNotificationService = whatsAppNotificationService;
        _executionInterval = executionInterval ?? TimeSpan.FromHours(24);
        _telegramRecipient = telegramRecipient ?? "000000";
        _whatsAppRecipient = whatsAppRecipient ?? "000000";
    }

    /// <summary>
    /// Ejecuta el ciclo infinito que cada 24 horas refresca la información.
    /// </summary>
    public async Task RunAsync(UserSearchRequest request, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await RunSingleIterationAsync(request, cancellationToken).ConfigureAwait(false);
            await Task.Delay(_executionInterval, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Ejecuta una iteración de recolección, ordenamiento y notificación.
    /// </summary>
    public async Task RunSingleIterationAsync(UserSearchRequest request, CancellationToken cancellationToken)
    {
        var allTrips = new List<Trip>();
        var searchTasks = _flightServices.Select(service => service.SearchTripsAsync(request, cancellationToken)).ToList();
        var searchResults = await Task.WhenAll(searchTasks).ConfigureAwait(false);
        foreach (var result in searchResults)
        {
            allTrips.AddRange(result);
        }

        var scrapingFindings = await _webScrapingService.ScrapeTripsAsync(request, cancellationToken).ConfigureAwait(false);
        allTrips.AddRange(scrapingFindings.Select(f => f.Trip));

        allTrips = allTrips
            .Where(trip => trip.Stops <= request.MaxStops)
            .Distinct(_tripComparer)
            .ToList();

        var orderedTrips = OrderTrips(allTrips, request.Priority).ToList();
        var persistedTrips = await _tripPersistenceService.LoadAsync(cancellationToken).ConfigureAwait(false);
        var mergedTrips = persistedTrips
            .Concat(orderedTrips)
            .Distinct(_tripComparer)
            .ToList();

        var topTrips = OrderTrips(mergedTrips, request.Priority)
            .Take(request.MaxTopResults)
            .ToList();

        var previousTop = persistedTrips.Take(request.MaxTopResults).ToList();
        var hasChanges = !previousTop.SequenceEqual(topTrips, _tripComparer);

        if (hasChanges && topTrips.Any())
        {
            var message = BuildNotificationMessage(request, topTrips);
            await _telegramNotificationService.SendMessageAsync(_telegramRecipient, message, cancellationToken).ConfigureAwait(false);

            // WhatsApp permanece en pausa, pero se registra el mensaje para su futuro envío.
            await _whatsAppNotificationService.SendMessageAsync(
                _whatsAppRecipient,
                "WhatsApp suspendido temporalmente. Mensaje en cola:\n" + message,
                cancellationToken).ConfigureAwait(false);
        }

        await _tripPersistenceService.SaveAsync(topTrips, cancellationToken).ConfigureAwait(false);
    }

    private static IEnumerable<Trip> OrderTrips(IEnumerable<Trip> trips, TripPriority priority) => priority switch
    {
        TripPriority.Precio => trips.OrderBy(t => t.TotalPrice).ThenBy(t => t.TotalDuration),
        TripPriority.Tiempo => trips.OrderBy(t => t.TotalDuration).ThenBy(t => t.TotalPrice),
        TripPriority.Escalas => trips.OrderBy(t => t.Stops).ThenBy(t => t.TotalPrice),
        _ => trips
    };

    private static string BuildNotificationMessage(UserSearchRequest request, IReadOnlyList<Trip> trips)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Cambios detectados en los mejores viajes {request.OriginCity} -> {request.DestinationCity}.");
        builder.AppendLine($"Criterio: {request.Priority}.");
        builder.AppendLine();

        for (var i = 0; i < trips.Count; i++)
        {
            var trip = trips[i];
            builder.AppendLine($"#{i + 1} - {trip.DepartureAirport.City} ({trip.DepartureAirport.AirportCode}) -> {trip.ArrivalAirport.City} ({trip.ArrivalAirport.AirportCode})");
            builder.AppendLine($"      Precio: {trip.Currency} {trip.TotalPrice.ToString("F2", CultureInfo.InvariantCulture)} | Escalas: {trip.Stops} | Duración: {trip.TotalDuration}");
            foreach (var flight in trip.Flights)
            {
                builder.AppendLine($"      Vuelo {flight.Airline}: {flight.DepartureAirport.AirportCode} {flight.DepartureTime:dd/MM HH:mm} -> {flight.ArrivalAirport.AirportCode} {flight.ArrivalTime:dd/MM HH:mm} | Precio: {flight.Currency} {flight.Price.ToString("F2", CultureInfo.InvariantCulture)} | Equipaje: {(flight.BaggageIncluded ? "Incluido" : flight.BaggageNotes)}");
            }
            if (i < trips.Count - 1)
            {
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }
}
