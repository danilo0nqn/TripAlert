using System.Globalization;
using TripAlert.Core.Automation;
using TripAlert.Core.Models;
using TripAlert.Core.Services.FlightApis;
using TripAlert.Core.Services.Notifications;
using TripAlert.Core.Services.Persistence;
using TripAlert.Core.Services.WebScraping;

var cancellationSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationSource.Cancel();
};

Console.WriteLine("=== TripAlert ===");
Console.WriteLine("Todos los valores monetarios se expresarán en dólares estadounidenses (USD).\n");

var originCity = ReadRequiredValue("Ciudad de partida: ");
var destinationCity = ReadRequiredValue("Ciudad de llegada: ");
var fromDate = ReadDate("Fecha de inicio de búsqueda (AAAA-MM-DD): ");
var toDate = ReadDate("Fecha de fin de búsqueda (AAAA-MM-DD): ");

while (toDate < fromDate)
{
    Console.WriteLine("La fecha de fin no puede ser anterior a la fecha de inicio. Inténtelo nuevamente.");
    toDate = ReadDate("Fecha de fin de búsqueda (AAAA-MM-DD): ");
}

var priority = ReadPriority();

var request = new UserSearchRequest
{
    OriginCity = originCity,
    DestinationCity = destinationCity,
    DepartureFrom = fromDate,
    DepartureTo = toDate,
    Priority = priority,
    Currency = "USD"
};

using var skyscannerService = new SkyscannerFlightSearchService();
var flightServices = new IFlightSearchService[]
{
    skyscannerService,
    new KiwiFlightSearchService(),
    new AmadeusFlightSearchService(),
    new GoogleFlightsSearchService()
};

var telegramBotToken = Environment.GetEnvironmentVariable("TRIPALERT_TELEGRAM_BOT_TOKEN");
if (string.IsNullOrWhiteSpace(telegramBotToken))
{
    Console.WriteLine("Debe configurar la variable de entorno TRIPALERT_TELEGRAM_BOT_TOKEN con el token del bot de Telegram.");
    return;
}

var telegramChatId = Environment.GetEnvironmentVariable("TRIPALERT_TELEGRAM_CHAT");
if (string.IsNullOrWhiteSpace(telegramChatId))
{
    Console.WriteLine("Debe configurar la variable de entorno TRIPALERT_TELEGRAM_CHAT con el identificador del chat de destino.");
    return;
}

using var telegramService = new TelegramNotificationService(telegramBotToken);
var whatsAppService = new DeferredWhatsAppNotificationService();
var persistencePath = Path.Combine(AppContext.BaseDirectory, "data", "best_trips.json");
var persistenceService = new JsonTripPersistenceService(persistencePath);
var webScrapingService = new SimpleWebScrapingService(flightServices);
var automation = new TripAlertAutomation(
    flightServices,
    webScrapingService,
    persistenceService,
    telegramService,
    whatsAppService,
    TimeSpan.FromHours(24),
    telegramChatId,
    Environment.GetEnvironmentVariable("TRIPALERT_WHATSAPP_NUMBER") ?? "000000");

Console.WriteLine();
Console.WriteLine("Automatización iniciada. Presione Ctrl+C para detener.");
await automation.RunAsync(request, cancellationSource.Token);

static string ReadRequiredValue(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var value = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value.Trim();
        }

        Console.WriteLine("El valor ingresado no puede estar vacío.");
    }
}

static DateTime ReadDate(string prompt)
{
    var formats = new[] { "yyyy-MM-dd", "yyyy/MM/dd", "dd/MM/yyyy" };
    while (true)
    {
        Console.Write(prompt);
        var value = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(value) &&
            DateTime.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        Console.WriteLine("Formato de fecha inválido. Utilice por ejemplo 2024-10-02.");
    }
}

static TripPriority ReadPriority()
{
    while (true)
    {
        Console.Write("Prioridad (Precio/Tiempo/Escalas): ");
        var value = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse<TripPriority>(value.Trim(), true, out var priority))
        {
            return priority;
        }

        Console.WriteLine("La prioridad debe ser Precio, Tiempo o Escalas.");
    }
}
