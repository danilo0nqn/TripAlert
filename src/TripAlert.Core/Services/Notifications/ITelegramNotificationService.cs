namespace TripAlert.Core.Services.Notifications;

/// <summary>
/// Servicio encargado de enviar notificaciones vía Telegram.
/// </summary>
public interface ITelegramNotificationService
{
    /// <summary>
    /// Envía un mensaje al destinatario indicado.
    /// </summary>
    Task SendMessageAsync(string recipient, string message, CancellationToken cancellationToken);
}
