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
    /// Gets or sets the kind of server.
    /// </summary>
    public DaxServerType? ServerType { get; set; }

    /// <summary>
    /// Gets or sets the mode of the Analysis Services server.
    /// </summary>
    public DaxServerMode? ServerMode { get; set; }

    /// <summary>
    /// Gets or sets the edition of the Analysis Services server.
    /// </summary>
    public DaxServerEdition? ServerEdition { get; set; }

    /// <summary>
    /// Gets or sets where the server runs.
    /// </summary>
    public DaxServerLocation? ServerLocation { get; set; }

    /// <summary>
    /// Gets or sets the version of the server, such as <c>16.0.143.21</c>.
    /// </summary>
    public string? ServerVersion { get; set; }

    /// <summary>
    /// Gets or sets the compatibility level of the database, such as <c>1600</c>.
    /// </summary>
    public int? CompatibilityLevel { get; set; }
}
