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

if (args.Length != 5)
{
    PrintUsage();
    return;
}

if (!DateTime.TryParse(args[2], out var fromDate) || !DateTime.TryParse(args[3], out var toDate))
{
    Console.WriteLine("Las fechas deben tener un formato válido (por ejemplo, 2024-10-02).");
    return;
}

if (!Enum.TryParse<TripPriority>(args[4], ignoreCase: true, out var priority))
{
    Console.WriteLine("La prioridad debe ser Precio, Tiempo o Escalas.");
    return;
}

var request = new UserSearchRequest
{
    OriginCity = args[0],
    DestinationCity = args[1],
    DepartureFrom = fromDate,
    DepartureTo = toDate,
    Priority = priority
};

var flightServices = new IFlightSearchService[]
{
    new SkyscannerFlightSearchService(),
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

Console.WriteLine("Automatización iniciada. Presione Ctrl+C para detener.");
await automation.RunAsync(request, cancellationSource.Token);

static void PrintUsage()
{
    Console.WriteLine("Uso: TripAlert.App <ciudad_origen> <ciudad_destino> <fecha_desde> <fecha_hasta> <prioridad>");
    Console.WriteLine("Ejemplo: TripAlert.App Neuquén \"Buenos Aires\" 2024-10-02 2024-10-04 Precio");
}
