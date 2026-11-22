using System.Text.Json;

namespace Dax.Formatter.Tests.Fixtures;

/// <summary>
/// Reads the options of a fixture case, which are written with the names that every client language shares.
/// </summary>
internal static class FixtureOptions
{
    public static DaxFormatOptions Read(JsonElement options)
    {
        var result = new DaxFormatOptions
        {
            Kind = ReadEnum<DaxKind>(options, "kind"),
            LineStyle = ReadEnum<DaxLineStyle>(options, "lineStyle"),
            SpacingStyle = ReadEnum<DaxSpacingStyle>(options, "spacingStyle"),
            SeparatorStyle = ReadEnum<DaxSeparatorStyle>(options, "separatorStyle"),
        };

        if (options.TryGetProperty("modelInfo", out var model))
        {
            result.ModelInfo = new DaxModelInfo
            {
                ServerName = ReadString(model, "serverName"),
                DatabaseName = ReadString(model, "databaseName"),
                ServerType = ReadString(model, "serverType"),
                ServerMode = ReadString(model, "serverMode"),
                ServerEdition = ReadString(model, "serverEdition"),
                ServerLocation = ReadString(model, "serverLocation"),
                ServerVersion = ReadString(model, "serverVersion"),
                CompatibilityLevel = model.TryGetProperty("compatibilityLevel", out var level) ? level.GetInt32() : null,
            };
        }

        return result;
    }

    public static DaxKind ReadKind(JsonElement testCase)
    {
        return ReadEnum<DaxKind>(testCase, "kind");
    }

    private static TEnum ReadEnum<TEnum>(JsonElement element, string name)
        where TEnum : struct, Enum
    {
        var value = ReadString(element, name);
        if (value is null)
        {
            return default;
        }

        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var result)
            ? result
            : throw new InvalidOperationException("The fixture has the unknown value '" + value + "'.");
    }

    private static string? ReadString(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) ? value.GetString() : null;
    }
}
