namespace Dax.Formatter;

/// <summary>
/// Where the server runs, for <see cref="DaxModelInfo.ServerLocation"/>. The members have the names and values of
/// <c>Microsoft.AnalysisServices.ServerLocation</c>.
/// </summary>
public enum DaxServerLocation
{
    /// <summary>A server on premises.</summary>
    OnPremise,

    /// <summary>A server in Azure.</summary>
    Azure,
}
