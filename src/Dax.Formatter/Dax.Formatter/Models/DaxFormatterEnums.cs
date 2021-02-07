namespace Dax.Formatter.Models
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
        SSDT
    }

    
}
