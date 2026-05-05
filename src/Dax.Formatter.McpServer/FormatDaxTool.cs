namespace Dax.Formatter.McpServer;

using Dax.Formatter;
using Dax.Formatter.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Reflection;

[McpServerToolType]
internal static class FormatDaxTool
{
    [McpServerTool(
        Name = "format_dax",
        Title = "Format DAX expressions",
        ReadOnly = true,
        Idempotent = true,
        OpenWorld = true,
        Destructive = false)]
    [Description("""
        Formats DAX (Data Analysis Expressions) code. DAX comments are preserved. Supports both DAX queries and DAX expressions.

        WHEN TO CALL:
        - The user asks to format, beautify, prettify or normalize DAX code.

        RETURNS:
        An array of responses, one per input in the same order:
        {
          Formatted: string,
          Errors: [{ Line: int, Column: int, Message: string }]
        }
        A format failure (invalid DAX) results in null `Formatted` and a non-empty `Errors` array.
        A successful format results in a non-null `Formatted` and an empty or null `Errors` array.

        NOTE:
        This tool calls an external HTTP service. Always batch multiple snippets
        in a single call — looping the tool wastes round-trips and is rate-limited upstream.
        """)]
    public static async Task<DaxFormatterResponse[]> FormatDax(
        IDaxFormatterClient client,
        [Description("""
            The DAX expressions to format. Each array element must be a complete,
            independent piece of DAX code — either a DAX query or a DAX expression.
            """)]
        string[] expressions,
        [Description("List separator character.")]
        char listSeparator = ',',
        [Description("Decimal separator character.")]
        char decimalSeparator = '.',
        [Description("""
            Controls how arguments and sub-expressions are wrapped across lines.
            - 'LongLine': keeps arguments compact horizontally where readable.
            - 'ShortLine': breaks each argument onto its own line for maximum vertical clarity.
            """)]
        DaxFormatterLineStyle lineStyle = DaxFormatterLineStyle.LongLine,
        [Description($"""
            Controls spacing between function names and their opening parentheses.
            - 'SpaceAfterFunction': adds a space for readability, e.g. 'SUM (x)'.
            - 'NoSpaceAfterFunction': removes the space for compactness, e.g. 'SUM(x)'.
            """)]
        DaxFormatterSpacingStyle spacingStyle = DaxFormatterSpacingStyle.SpaceAfterFunction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expressions);

        if (expressions.Length == 0)
            return [];

        var request = new DaxFormatterMultipleRequest();
        {
            // Set formatting options
            request.DecimalSeparator = decimalSeparator;
            request.ListSeparator = listSeparator;
            request.MaxLineLength = lineStyle;
            request.SkipSpaceAfterFunctionName = spacingStyle;
            // Add caller info
            request.CallerApp = "Dax.Formatter.McpServer";
            request.CallerVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        }
        request.Dax.AddRange(expressions);

        var responses = await client.FormatAsync(request, cancellationToken).ConfigureAwait(false);
        return [.. responses];
    }
}
