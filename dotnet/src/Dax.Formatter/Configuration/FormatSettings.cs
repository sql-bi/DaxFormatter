using System.Globalization;
using Dax.Formatter.Wire;

namespace Dax.Formatter.Configuration;

/// <summary>
/// The validated, immutable copy of a <see cref="DaxFormatOptions"/>, with its values in the form the service
/// takes. The names of the server and of the database are kept only as hashes.
/// </summary>
internal sealed class FormatSettings
{
    private const string NotDefined = " is not a value of its enumeration.";

    private FormatSettings(DaxFormatOptions options)
    {
        Kind = options.Kind switch
        {
            DaxKind.Auto or DaxKind.Expression => options.Kind,
            _ => throw new ArgumentException(nameof(options.Kind) + NotDefined, nameof(options)),
        };

        MaxLineLength = options.LineStyle switch
        {
            DaxLineStyle.LongLine => 0,
            DaxLineStyle.ShortLine => 1,
            _ => throw new ArgumentException(nameof(options.LineStyle) + NotDefined, nameof(options)),
        };

        SkipSpaceAfterFunctionName = options.SpacingStyle switch
        {
            DaxSpacingStyle.SpaceAfterFunction => false,
            DaxSpacingStyle.NoSpaceAfterFunction => true,
            _ => throw new ArgumentException(nameof(options.SpacingStyle) + NotDefined, nameof(options)),
        };

        (ListSeparator, DecimalSeparator) = options.SeparatorStyle switch
        {
            DaxSeparatorStyle.CommaAndPoint => (",", "."),
            DaxSeparatorStyle.SemicolonAndComma => (";", ","),
            _ => throw new ArgumentException(nameof(options.SeparatorStyle) + NotDefined, nameof(options)),
        };

        var model = options.ModelInfo;
        if (model is null)
        {
            return;
        }

        ServerNameHash = model.ServerName is null ? null : NameHash.Compute(model.ServerName);
        DatabaseNameHash = model.DatabaseName is null ? null : NameHash.Compute(model.DatabaseName);
        ServerType = model.ServerType switch
        {
            null => null,
            DaxServerType.AnalysisServices => "SSAS",
            DaxServerType.PowerBIDesktop => "PBI Desktop",
            DaxServerType.PowerBIReportServer => "PBI Report Server",
            DaxServerType.PowerPivot => "PowerPivot",
            DaxServerType.Ssdt => "SSDT",
            DaxServerType.AzureAnalysisServices => "AzureAS",
            DaxServerType.PowerBIService => "PBI Service",
            DaxServerType.Offline => "Offline",
            _ => throw new ArgumentException(nameof(model.ServerType) + NotDefined, nameof(options)),
        };

        // For these the service takes the name of the member, which is also the name in Analysis Services.
        ServerMode = model.ServerMode switch
        {
            null => null,
            { } mode when IsDefined(mode) => mode.ToString(),
            _ => throw new ArgumentException(nameof(model.ServerMode) + NotDefined, nameof(options)),
        };

        ServerEdition = model.ServerEdition switch
        {
            null => null,
            { } edition when IsDefined(edition) => edition.ToString(),
            _ => throw new ArgumentException(nameof(model.ServerEdition) + NotDefined, nameof(options)),
        };

        ServerLocation = model.ServerLocation switch
        {
            null => null,
            { } location when IsDefined(location) => location.ToString(),
            _ => throw new ArgumentException(nameof(model.ServerLocation) + NotDefined, nameof(options)),
        };
        ServerVersion = model.ServerVersion;
        DatabaseCompatibilityLevel = model.CompatibilityLevel?.ToString(CultureInfo.InvariantCulture);
    }

    public DaxKind Kind { get; }

    public int MaxLineLength { get; }

    public bool SkipSpaceAfterFunctionName { get; }

    public string ListSeparator { get; }

    public string DecimalSeparator { get; }

    public string? ServerNameHash { get; }

    public string? DatabaseNameHash { get; }

    public string? ServerType { get; }

    public string? ServerMode { get; }

    public string? ServerEdition { get; }

    public string? ServerLocation { get; }

    public string? ServerVersion { get; }

    public string? DatabaseCompatibilityLevel { get; }

    /// <exception cref="ArgumentException">An option is not a value of its enumeration.</exception>
    public static FormatSettings From(DaxFormatOptions? options)
    {
        return new FormatSettings(options ?? new DaxFormatOptions());
    }

    private static bool IsDefined<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
#if NET
        return Enum.IsDefined(value);
#else
        return Enum.IsDefined(typeof(TEnum), value);
#endif
    }
}
