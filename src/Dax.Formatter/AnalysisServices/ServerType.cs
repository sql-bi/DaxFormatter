namespace Dax.Formatter.AnalysisServices
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum ServerType
    {
        [EnumMember(Value = "SSAS")]
        AnalysisServices,

        [EnumMember(Value = "PBI Desktop")]
        PowerBIDesktop,

        [EnumMember(Value = "PBI Report Server")]
        PowerBIReportServer,

        [EnumMember(Value = "PowerPivot")]
        PowerPivot,

        [EnumMember(Value = "SSDT")]
        SSDT,

        [EnumMember(Value = "AzureAS")]
        AzureAnalysisServices,

        [EnumMember(Value = "PBI Service")]
        PowerBIService,

        [EnumMember(Value = "Offline")]
        Offline

    }
}
