namespace Dax.Formatter;

/// <summary>
/// The kind of server the model is on, for <see cref="DaxModelInfo.ServerType"/>.
/// </summary>
public enum DaxServerType
{
    /// <summary>SQL Server Analysis Services.</summary>
    AnalysisServices,

    /// <summary>Power BI Desktop.</summary>
    PowerBIDesktop,

    /// <summary>Power BI Report Server.</summary>
    PowerBIReportServer,

    /// <summary>Power Pivot for Excel.</summary>
    PowerPivot,

    /// <summary>The integrated workspace of SQL Server Data Tools.</summary>
    Ssdt,

    /// <summary>Azure Analysis Services.</summary>
    AzureAnalysisServices,

    /// <summary>The Power BI service.</summary>
    PowerBIService,

    /// <summary>A model that is not on a server, such as a file.</summary>
    Offline,
}
