namespace TripAlert.Core.Services.Notifications;

/// <summary>
/// Implementación básica de Telegram que registra los mensajes en la consola.
/// </summary>
public sealed class ConsoleTelegramNotificationService : ITelegramNotificationService
{
    /// <inheritdoc />
    public Task SendMessageAsync(string recipient, string message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[Telegram -> {recipient}] {message}");
        return Task.CompletedTask;
    }
}
