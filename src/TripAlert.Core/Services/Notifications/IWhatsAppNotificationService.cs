namespace TripAlert.Core.Services.Notifications;

/// <summary>
/// Servicio que permitirá enviar mensajes por WhatsApp en el futuro.
/// </summary>
public interface IWhatsAppNotificationService
{
    /// <summary>
    /// Envía un mensaje de WhatsApp al destinatario.
    /// </summary>
    Task SendMessageAsync(string recipient, string message, CancellationToken cancellationToken);
}
