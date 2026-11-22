using System.Diagnostics.CodeAnalysis;
using Dax.Formatter.Expressions;
using Dax.Formatter.Wire;

namespace Dax.Formatter.Results;

/// <summary>
/// Builds the result of one text from the text that the caller gave and from what the service answered for it.
/// </summary>
internal static class ResultMapper
{
    /// <param name="originalText">The text that the caller gave.</param>
    /// <param name="header">The header that the client sent the text under, if any.</param>
    /// <param name="item">What the service answered for the text.</param>
    /// <param name="result">The result, when the answer can be used.</param>
    /// <returns><see langword="false"/> when the formatted text lacks the header that the client added.</returns>
    public static bool TryMap(
        string originalText,
        ExpressionHeader? header,
        FormatResponseItem item,
        [NotNullWhen(true)] out DaxFormatResult? result)
    {
        // With errors the service has nothing formatted to return, so the formatted text is not read.
        if (item.Errors is { Count: > 0 } errors)
        {
            result = DaxFormatResult.Failure(originalText, MapErrors(originalText, header, errors));
            return true;
        }

        var formatted = item.Formatted ?? string.Empty;
        if (header is not null)
        {
            if (!HeaderRemover.TryRemove(formatted, header, out var body))
            {
                result = null;
                return false;
            }

            formatted = body;
        }

        result = DaxFormatResult.Success(originalText, RemoveFinalLineBreak(formatted));
        return true;
    }

    private static List<DaxSyntaxError> MapErrors(
        string originalText,
        ExpressionHeader? header,
        List<FormatResponseError?> errors)
    {
        var mapped = new List<DaxSyntaxError>(errors.Count);
        foreach (var error in errors)
        {
            var reportedLine = error?.Line ?? 0;
            var reportedColumn = error?.Column ?? 0;
            var (line, column) = ErrorLocator.Locate(originalText, header is not null, reportedLine, reportedColumn);
            mapped.Add(new DaxSyntaxError(line, column, error?.Message ?? string.Empty));
        }

        return mapped;
    }

    private static string RemoveFinalLineBreak(string text)
    {
        if (text.Length == 0 || text[text.Length - 1] != '\n')
        {
            return text;
        }

        var end = text.Length - 1;
        if (end > 0 && text[end - 1] == '\r')
        {
            end--;
        }

        return text.Substring(0, end);
    }
}
