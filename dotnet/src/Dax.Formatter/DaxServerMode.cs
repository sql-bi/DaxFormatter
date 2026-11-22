namespace Dax.Formatter;

/// <summary>
/// The mode of the Analysis Services server, for <see cref="DaxModelInfo.ServerMode"/>. The members have the names
/// and values of <c>Microsoft.AnalysisServices.ServerMode</c>.
/// </summary>
public enum DaxServerMode
{
    /// <summary>The multidimensional mode.</summary>
    Multidimensional,

    /// <summary>The SharePoint integration mode.</summary>
    SharePoint,

    /// <summary>The tabular mode.</summary>
    Tabular,

    /// <summary>The default mode of the server.</summary>
    Default,
}
