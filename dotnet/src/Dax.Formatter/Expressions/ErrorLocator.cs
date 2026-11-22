namespace Dax.Formatter.Expressions;

/// <summary>
/// Turns the position of a syntax error, as the service reports it for the text it received, into a position in
/// the text that the caller gave.
/// </summary>
internal static class ErrorLocator
{
    private const int TabStop = 8;

    /// <param name="originalText">The text that the caller gave.</param>
    /// <param name="hasHeader">Whether the client sent the text under a header line.</param>
    /// <param name="line">The zero-based line that the service reports.</param>
    /// <param name="column">The column that the service reports: tabs count up to the next tab stop.</param>
    /// <returns>The zero-based line and the UTF-16 index in that line.</returns>
    public static (int Line, int Column) Locate(string originalText, bool hasHeader, int line, int column)
    {
        var targetLine = Math.Max(line, 0);
        if (hasHeader)
        {
            // The header is not part of the text of the caller: an error there can only point to its start.
            if (targetLine == 0)
            {
                return (0, 0);
            }

            targetLine--;
        }

        var currentLine = 0;
        var lineStart = 0;
        while (currentLine < targetLine)
        {
            var lineFeed = originalText.IndexOf('\n', lineStart);
            if (lineFeed < 0)
            {
                break;
            }

            lineStart = lineFeed + 1;
            currentLine++;
        }

        var lineEnd = FindLineEnd(originalText, lineStart);
        if (currentLine < targetLine)
        {
            // The service reports the end of the input as the line after the last one.
            return (currentLine, lineEnd - lineStart);
        }

        return (currentLine, ToIndex(originalText, lineStart, lineEnd, Math.Max(column, 0)) - lineStart);
    }

    // The end of the line, before its line feed and the carriage return that can precede it.
    private static int FindLineEnd(string text, int lineStart)
    {
        var lineFeed = text.IndexOf('\n', lineStart);
        var end = lineFeed < 0 ? text.Length : lineFeed;
        if (end > lineStart && text[end - 1] == '\r')
        {
            end--;
        }

        return end;
    }

    // The service counts a tab as a jump to the next tab stop and gives a carriage return no width.
    private static int ToIndex(string text, int lineStart, int lineEnd, int targetColumn)
    {
        var column = 0;
        for (var index = lineStart; index < lineEnd; index++)
        {
            var character = text[index];
            if (character == '\r')
            {
                continue;
            }

            if (column >= targetColumn)
            {
                return index;
            }

            column = character == '\t' ? ((column / TabStop) + 1) * TabStop : column + 1;
        }

        return lineEnd;
    }
}
