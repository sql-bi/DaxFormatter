namespace Dax.Formatter.Wire;

/// <summary>
/// The JSON body of a request to the endpoint that formats several texts.
/// </summary>
internal sealed class FormatRequestBody
{
    public IReadOnlyList<string> Dax { get; set; } = [];

    public int MaxLineLength { get; set; }

    public bool SkipSpaceAfterFunctionName { get; set; }

    public string ListSeparator { get; set; } = ",";

    public string DecimalSeparator { get; set; } = ".";

    public string? ServerName { get; set; }

    public string? DatabaseName { get; set; }

    public string? ServerType { get; set; }

    public string? ServerMode { get; set; }

    public string? ServerEdition { get; set; }

    public string? ServerLocation { get; set; }

    public string? ServerVersion { get; set; }

    public string? DatabaseCompatibilityLevel { get; set; }

    public string? CallerApp { get; set; }

    public string? CallerVersion { get; set; }
}
