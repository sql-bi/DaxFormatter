namespace Dax.Formatter;

/// <summary>
/// The description of the model the DAX texts come from, for the anonymous usage statistics of the service.
/// Every property is optional, and none changes the formatting.
/// </summary>
public sealed class DaxModelInfo
{
    /// <summary>
    /// Gets or sets the name of the server. The client sends only its SHA-256 hash, never the name.
    /// </summary>
    public string? ServerName { get; set; }

    /// <summary>
    /// Gets or sets the name of the database. The client sends only its SHA-256 hash, never the name.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the kind of server. <see cref="DaxServerTypes"/> has the values the service knows.
    /// </summary>
    public string? ServerType { get; set; }

    /// <summary>
    /// Gets or sets the mode of the Analysis Services server, such as <c>Tabular</c>.
    /// </summary>
    public string? ServerMode { get; set; }

    /// <summary>
    /// Gets or sets the edition of the Analysis Services server, such as <c>Enterprise64</c>.
    /// </summary>
    public string? ServerEdition { get; set; }

    /// <summary>
    /// Gets or sets where the server runs, such as <c>OnPremise</c> or <c>Azure</c>.
    /// </summary>
    public string? ServerLocation { get; set; }

    /// <summary>
    /// Gets or sets the version of the server, such as <c>16.0.143.21</c>.
    /// </summary>
    public string? ServerVersion { get; set; }

    /// <summary>
    /// Gets or sets the compatibility level of the database, such as <c>1600</c>.
    /// </summary>
    public int? CompatibilityLevel { get; set; }
}
