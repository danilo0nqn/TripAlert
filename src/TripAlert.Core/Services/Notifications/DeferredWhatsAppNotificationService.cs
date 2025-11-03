namespace TripAlert.Core.Services.Notifications;

/// <summary>
/// Implementación temporal para WhatsApp que deja registro de los mensajes pendientes.
/// </summary>
public sealed class DeferredWhatsAppNotificationService : IWhatsAppNotificationService
{
    private readonly List<(string Recipient, string Message, DateTime CreatedAt)> _pendingMessages = new();

    /// <summary>
    /// Mensajes pendientes de envío.
    /// </summary>
    public IReadOnlyList<(string Recipient, string Message, DateTime CreatedAt)> PendingMessages => _pendingMessages;

    /// <inheritdoc />
    public Task SendMessageAsync(string recipient, string message, CancellationToken cancellationToken)
    {
        _pendingMessages.Add((recipient, message, DateTime.UtcNow));
        Console.WriteLine($"[WhatsApp (pendiente) -> {recipient}] {message}");
        return Task.CompletedTask;
    }
}
