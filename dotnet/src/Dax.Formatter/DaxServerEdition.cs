namespace Dax.Formatter;

/// <summary>
/// The edition of the Analysis Services server, for <see cref="DaxModelInfo.ServerEdition"/>. The members have the
/// names and values of <c>Microsoft.AnalysisServices.ServerEdition</c>.
/// </summary>
public enum DaxServerEdition
{
    /// <summary>The Standard edition.</summary>
    Standard,

    /// <summary>The 64-bit Standard edition.</summary>
    Standard64,

    /// <summary>The Enterprise edition.</summary>
    Enterprise,

    /// <summary>The 64-bit Enterprise edition.</summary>
    Enterprise64,

    /// <summary>The Developer edition.</summary>
    Developer,

    /// <summary>The 64-bit Developer edition.</summary>
    Developer64,

    /// <summary>The Evaluation edition.</summary>
    Evaluation,

    /// <summary>The 64-bit Evaluation edition.</summary>
    Evaluation64,

    /// <summary>A local cube.</summary>
    LocalCube,

    /// <summary>A 64-bit local cube.</summary>
    LocalCube64,

    /// <summary>The Business Intelligence edition.</summary>
    BusinessIntelligence,

    /// <summary>The 64-bit Business Intelligence edition.</summary>
    BusinessIntelligence64,

    /// <summary>The Enterprise Core edition.</summary>
    EnterpriseCore,

    /// <summary>The 64-bit Enterprise Core edition.</summary>
    EnterpriseCore64,

    /// <summary>The Standard Developer edition.</summary>
    StandardDeveloper,

    /// <summary>The 64-bit Standard Developer edition.</summary>
    StandardDeveloper64,
}
