namespace Dax.Formatter.McpServer;

using System.ComponentModel;
using Dax.Formatter;
using Dax.Formatter.AnalysisServices;
using Dax.Formatter.Models;
using ModelContextProtocol.Server;

public sealed record FormatError(int? Line, int? Column, string? Message);

public sealed record FormatResult(string? Formatted, FormatError[] Errors);

[McpServerToolType]
public static class DaxFormatterTools
{
    [McpServerTool, Description("Format a single DAX expression using the daxformatter.com web service.")]
    public static async Task<FormatResult> FormatDax(
        IDaxFormatterClient client,
        [Description("The DAX expression to format.")] string expression,
        [Description("List separator character (default ',').")] char? listSeparator = null,
        [Description("Decimal separator character (default '.').")] char? decimalSeparator = null,
        [Description("Line style: 'LongLine' or 'ShortLine'.")] string? maxLineLength = null,
        [Description("Spacing style: 'BestPractice', 'SpaceAfterFunction', or 'NoSpaceAfterFunction'.")] string? skipSpaceAfterFunctionName = null,
        [Description("Server type, e.g. 'Tabular', 'PowerBIDesktop', 'AnalysisServices', 'PowerBIService', 'AzureAnalysisServices', 'PowerBIReportServer', 'PowerPivot', 'SSDT', 'Offline'.")] string? serverType = null,
        [Description("Server mode: 'Tabular', 'Multidimensional', 'SharePoint', 'Default'.")] string? serverMode = null,
        [Description("Server edition: 'Enterprise64', 'Developer64', 'Standard64'.")] string? serverEdition = null,
        [Description("Server location: 'OnPremise' or 'Azure'.")] string? serverLocation = null,
        [Description("Server version string, e.g. '14.0.800.192'.")] string? serverVersion = null,
        [Description("Database compatibility level as a string, e.g. '1500'.")] string? databaseCompatibilityLevel = null,
        [Description("Server name (hashed before sending).")] string? serverName = null,
        [Description("Database name (hashed before sending).")] string? databaseName = null,
        CancellationToken cancellationToken = default)
    {
        var request = new DaxFormatterSingleRequest { Dax = expression };
        ApplyOptions(request, listSeparator, decimalSeparator, maxLineLength, skipSpaceAfterFunctionName,
            serverType, serverMode, serverEdition, serverLocation, serverVersion,
            databaseCompatibilityLevel, serverName, databaseName);

        var response = await client.FormatAsync(request, cancellationToken).ConfigureAwait(false);
        return ToResult(response);
    }

    [McpServerTool, Description("Format multiple DAX expressions in a single call using the daxformatter.com web service. Results are returned in input order.")]
    public static async Task<FormatResult[]> FormatDaxBatch(
        IDaxFormatterClient client,
        [Description("The DAX expressions to format.")] string[] expressions,
        [Description("List separator character (default ',').")] char? listSeparator = null,
        [Description("Decimal separator character (default '.').")] char? decimalSeparator = null,
        [Description("Line style: 'LongLine' or 'ShortLine'.")] string? maxLineLength = null,
        [Description("Spacing style: 'BestPractice', 'SpaceAfterFunction', or 'NoSpaceAfterFunction'.")] string? skipSpaceAfterFunctionName = null,
        [Description("Server type, e.g. 'Tabular', 'PowerBIDesktop', 'AnalysisServices', 'PowerBIService', 'AzureAnalysisServices', 'PowerBIReportServer', 'PowerPivot', 'SSDT', 'Offline'.")] string? serverType = null,
        [Description("Server mode: 'Tabular', 'Multidimensional', 'SharePoint', 'Default'.")] string? serverMode = null,
        [Description("Server edition: 'Enterprise64', 'Developer64', 'Standard64'.")] string? serverEdition = null,
        [Description("Server location: 'OnPremise' or 'Azure'.")] string? serverLocation = null,
        [Description("Server version string, e.g. '14.0.800.192'.")] string? serverVersion = null,
        [Description("Database compatibility level as a string, e.g. '1500'.")] string? databaseCompatibilityLevel = null,
        [Description("Server name (hashed before sending).")] string? serverName = null,
        [Description("Database name (hashed before sending).")] string? databaseName = null,
        CancellationToken cancellationToken = default)
    {
        var request = new DaxFormatterMultipleRequest();
        request.Dax.AddRange(expressions);
        ApplyOptions(request, listSeparator, decimalSeparator, maxLineLength, skipSpaceAfterFunctionName,
            serverType, serverMode, serverEdition, serverLocation, serverVersion,
            databaseCompatibilityLevel, serverName, databaseName);

        var responses = await client.FormatAsync(request, cancellationToken).ConfigureAwait(false);
        var results = new FormatResult[responses.Count];
        for (var i = 0; i < responses.Count; i++)
            results[i] = ToResult(responses[i]);
        return results;
    }

    private static void ApplyOptions(
        DaxFormatterRequest request,
        char? listSeparator,
        char? decimalSeparator,
        string? maxLineLength,
        string? skipSpaceAfterFunctionName,
        string? serverType,
        string? serverMode,
        string? serverEdition,
        string? serverLocation,
        string? serverVersion,
        string? databaseCompatibilityLevel,
        string? serverName,
        string? databaseName)
    {
        if (listSeparator.HasValue) request.ListSeparator = listSeparator.Value;
        if (decimalSeparator.HasValue) request.DecimalSeparator = decimalSeparator.Value;
        if (maxLineLength is not null) request.MaxLineLength = ParseEnum<DaxFormatterLineStyle>(nameof(maxLineLength), maxLineLength);
        if (skipSpaceAfterFunctionName is not null) request.SkipSpaceAfterFunctionName = ParseEnum<DaxFormatterSpacingStyle>(nameof(skipSpaceAfterFunctionName), skipSpaceAfterFunctionName);
        if (serverType is not null) request.ServerType = ParseEnum<ServerType>(nameof(serverType), serverType);
        if (serverMode is not null) request.ServerMode = ParseEnum<ServerMode>(nameof(serverMode), serverMode);
        if (serverEdition is not null) request.ServerEdition = ParseEnum<ServerEdition>(nameof(serverEdition), serverEdition);
        if (serverLocation is not null) request.ServerLocation = ParseEnum<ServerLocation>(nameof(serverLocation), serverLocation);
        if (serverVersion is not null) request.ServerVersion = serverVersion;
        if (databaseCompatibilityLevel is not null) request.DatabaseCompatibilityLevel = databaseCompatibilityLevel;
        if (serverName is not null) request.ServerName = serverName;
        if (databaseName is not null) request.DatabaseName = databaseName;
    }

    private static T ParseEnum<T>(string paramName, string value) where T : struct, Enum
    {
        if (Enum.TryParse<T>(value, ignoreCase: true, out var parsed) && Enum.IsDefined(typeof(T), parsed))
            return parsed;

        var allowed = string.Join(", ", Enum.GetNames(typeof(T)));
        throw new ArgumentException($"Invalid value '{value}' for '{paramName}'. Allowed values: {allowed}.", paramName);
    }

    private static FormatResult ToResult(DaxFormatterResponse? response)
    {
        if (response is null)
            return new FormatResult(null, Array.Empty<FormatError>());

        var errors = response.Errors is { Count: > 0 }
            ? response.Errors.Select(e => new FormatError(e.Line, e.Column, e.Message)).ToArray()
            : Array.Empty<FormatError>();

        return new FormatResult(response.Formatted, errors);
    }
}
