using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace TripAlert.Core.Services.Notifications;

/// <summary>
/// Implementación concreta que envía mensajes mediante la API de Telegram Bot.
/// </summary>
public sealed class TelegramNotificationService : ITelegramNotificationService, IDisposable
{
    private const string ApiBaseUrl = "https://api.telegram.org";

    private readonly HttpClient _httpClient;
    private readonly bool _ownsClient;
    private readonly string _botToken;

    /// <summary>
    /// Crea una nueva instancia del servicio.
    /// </summary>
    /// <param name="botToken">Token del bot de Telegram.</param>
    /// <param name="httpClient">Cliente HTTP opcional. Si no se especifica se crea uno interno.</param>
    /// <exception cref="ArgumentException">Si el token es nulo o vacío.</exception>
    public TelegramNotificationService(string botToken, HttpClient? httpClient = null)
    {
        if (string.IsNullOrWhiteSpace(botToken))
        {
            throw new ArgumentException("El token del bot de Telegram es obligatorio.", nameof(botToken));
        }

        _botToken = botToken.Trim();
        _httpClient = httpClient ?? new HttpClient();
        _ownsClient = httpClient is null;
    }

    /// <inheritdoc />
    public async Task SendMessageAsync(string recipient, string message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(recipient))
        {
            throw new ArgumentException("El destinatario es obligatorio.", nameof(recipient));
        }

        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var payload = new TelegramMessagePayload(recipient.Trim(), message);
        var requestUri = $"{ApiBaseUrl}/bot{_botToken}/sendMessage";
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new TelegramNotificationException((int)response.StatusCode, responseBody);
        }
    }

    /// <summary>
    /// Libera los recursos asociados al cliente HTTP si es interno.
    /// </summary>
    public void Dispose()
    {
        if (_ownsClient)
        {
            _httpClient.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    private sealed record TelegramMessagePayload(string chat_id, string text)
    {
        public bool disable_web_page_preview { get; } = true;
    }
}

/// <summary>
/// Excepción lanzada cuando la API de Telegram responde con un error.
/// </summary>
public sealed class TelegramNotificationException : Exception
{
    /// <summary>
    /// Código de estado HTTP devuelto por la API.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Contenido textual devuelto por la API de Telegram.
    /// </summary>
    public string ResponseBody { get; }

    /// <summary>
    /// Crea una nueva instancia de la excepción.
    /// </summary>
    public TelegramNotificationException(int statusCode, string responseBody)
        : base($"Error al enviar mensaje por Telegram. Código: {statusCode}. Respuesta: {responseBody}")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
