using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Dax.Formatter.Expressions;

/// <summary>
/// Takes the header that the client added away from a formatted text, with the indentation that the service adds
/// under the header of a function.
/// </summary>
internal static class HeaderRemover
{
    private const string Indentation = "    ";

    /// <summary>
    /// Removes the header, or returns <see langword="false"/> when the formatted text does not start with it: such
    /// a text is not what the client asked for and must not reach the caller.
    /// </summary>
    public static bool TryRemove(string formatted, ExpressionHeader header, [NotNullWhen(true)] out string? body)
    {
        body = null;
        if (!formatted.StartsWith(header.Text, StringComparison.Ordinal))
        {
            return false;
        }

        var position = header.Text.Length;
        while (position < formatted.Length && formatted[position] == ' ')
        {
            position++;
        }

        // The service writes the body either on the line of the header or on the next one.
        var lineBreakLength = GetLineBreakLength(formatted, position);
        position += lineBreakLength;

        if (header.IndentsBody)
        {
            body = RemoveIndentation(formatted, position, startsLine: lineBreakLength > 0);
        }
        else
        {
            body = formatted.Substring(position);
        }

        return true;
    }

    private static int GetLineBreakLength(string text, int position)
    {
        if (position < text.Length && text[position] == '\n')
        {
            return 1;
        }

        if (position + 1 < text.Length && text[position] == '\r' && text[position + 1] == '\n')
        {
            return 2;
        }

        return 0;
    }

    // A line that starts inside a string, a name or a block comment is content: the service returns it as it
    // was sent, without indentation, so it is left as it is.
    private static string RemoveIndentation(string text, int start, bool startsLine)
    {
        var result = new StringBuilder(text.Length - start);
        var scanner = new DaxTextScanner(text, start);
        var copiedUpTo = start;
        var atLineStart = startsLine;

        while (true)
        {
            if (atLineStart && string.CompareOrdinal(text, scanner.Position, Indentation, 0, Indentation.Length) == 0)
            {
                result.Append(text, copiedUpTo, scanner.Position - copiedUpTo);
                scanner.Skip(Indentation.Length);
                copiedUpTo = scanner.Position;
            }

            var kind = scanner.Read();
            if (kind == DaxTokenKind.End)
            {
                break;
            }

            atLineStart = kind == DaxTokenKind.LineBreak;
        }

        result.Append(text, copiedUpTo, text.Length - copiedUpTo);
        return result.ToString();
    }
}
