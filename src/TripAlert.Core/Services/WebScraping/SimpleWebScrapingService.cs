using TripAlert.Core.Models;
using TripAlert.Core.Services.FlightApis;

namespace TripAlert.Core.Services.WebScraping;

/// <summary>
/// Implementación básica que reutiliza los servicios simulados para emular datos obtenidos mediante scraping.
/// </summary>
public sealed class SimpleWebScrapingService : IWebScrapingService
{
    private readonly IReadOnlyList<IFlightSearchService> _fallbackSources;

    /// <summary>
    /// Inicializa la instancia.
    /// </summary>
    public SimpleWebScrapingService(IEnumerable<IFlightSearchService> fallbackSources)
    {
        _fallbackSources = fallbackSources.ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WebScrapingFinding>> ScrapeTripsAsync(UserSearchRequest request, CancellationToken cancellationToken)
    {
        var findings = new List<WebScrapingFinding>();

        foreach (var source in _fallbackSources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var trips = await source.SearchTripsAsync(request, cancellationToken).ConfigureAwait(false);

            foreach (var trip in trips.Take(2))
            {
                findings.Add(new WebScrapingFinding(source.ProviderName + " Scraper", new Uri($"https://example.com/{source.ProviderName.ToLowerInvariant()}"), trip));
            }
        }

        return findings;
    }
}
