using System.Text.Json;
using Dax.Formatter.Configuration;

namespace Dax.Formatter.Wire;

/// <summary>
/// Writes the JSON body of a request as UTF-8 bytes, which every attempt of the request sends again.
/// </summary>
internal sealed class RequestBodyWriter
{
    private readonly CallerIdentity _caller;

    public RequestBodyWriter(CallerIdentity caller)
    {
        _caller = caller;
    }

    public byte[] Write(IReadOnlyList<string> dax, FormatSettings settings)
    {
        var body = new FormatRequestBody
        {
            Dax = dax,
            MaxLineLength = settings.MaxLineLength,
            SkipSpaceAfterFunctionName = settings.SkipSpaceAfterFunctionName,
            ListSeparator = settings.ListSeparator,
            DecimalSeparator = settings.DecimalSeparator,
            ServerName = settings.ServerNameHash,
            DatabaseName = settings.DatabaseNameHash,
            ServerType = settings.ServerType,
            ServerMode = settings.ServerMode,
            ServerEdition = settings.ServerEdition,
            ServerLocation = settings.ServerLocation,
            ServerVersion = settings.ServerVersion,
            DatabaseCompatibilityLevel = settings.DatabaseCompatibilityLevel,
            CallerApp = _caller.ApplicationName,
            CallerVersion = _caller.ApplicationVersion,
        };

        return JsonSerializer.SerializeToUtf8Bytes(body, WireJsonContext.Default.FormatRequestBody);
    }
}
