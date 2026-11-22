namespace Dax.Formatter;

/// <summary>
/// The values of <see cref="DaxModelInfo.ServerType"/> that the service knows.
/// </summary>
public static class DaxServerTypes
{
    /// <summary>SQL Server Analysis Services.</summary>
    public const string AnalysisServices = "SSAS";

    /// <summary>Power BI Desktop.</summary>
    public const string PowerBIDesktop = "PBI Desktop";

    /// <summary>Power BI Report Server.</summary>
    public const string PowerBIReportServer = "PBI Report Server";

    /// <summary>Power Pivot for Excel.</summary>
    public const string PowerPivot = "PowerPivot";

    /// <summary>The integrated workspace of SQL Server Data Tools.</summary>
    public const string Ssdt = "SSDT";

    /// <summary>Azure Analysis Services.</summary>
    public const string AzureAnalysisServices = "AzureAS";

    /// <summary>The Power BI service.</summary>
    public const string PowerBIService = "PBI Service";

    /// <summary>A model that is not on a server, such as a file.</summary>
    public const string Offline = "Offline";
}
