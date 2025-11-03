using TripAlert.Core.Models;

namespace TripAlert.Core.Services.WebScraping;

/// <summary>
/// Define el contrato para los procesos de web scraping.
/// </summary>
public interface IWebScrapingService
{
    /// <summary>
    /// Ejecuta el proceso de web scraping y retorna los viajes extraídos.
    /// </summary>
    Task<IReadOnlyList<WebScrapingFinding>> ScrapeTripsAsync(UserSearchRequest request, CancellationToken cancellationToken);
}
