namespace TripAlert.Core.Models;

/// <summary>
/// Resultado individual de un proceso de web scraping sobre una fuente específica.
/// </summary>
public sealed class WebScrapingFinding
{
    /// <summary>
    /// Crea el resultado de scraping.
    /// </summary>
    public WebScrapingFinding(string sourceName, Uri sourceUrl, Trip trip)
    {
        SourceName = sourceName;
        SourceUrl = sourceUrl;
        Trip = trip;
    }

    /// <summary>
    /// Nombre descriptivo de la fuente.
    /// </summary>
    public string SourceName { get; }

    /// <summary>
    /// URL desde donde se obtuvo la información.
    /// </summary>
    public Uri SourceUrl { get; }

    /// <summary>
    /// Viaje inferido a partir de los datos.
    /// </summary>
    public Trip Trip { get; }
}
