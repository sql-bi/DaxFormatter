using System.Text.Json.Nodes;

namespace Dax.Formatter.Tests.Fakes;

/// <summary>
/// Imitates the answers of the service without formatting anything: each text comes back as it was sent, with
/// the line breaks, the header layout and the indentation that the service gives. A text that contains
/// <see cref="ErrorMarker"/> comes back with a syntax error at the marker.
/// </summary>
internal static class EchoService
{
    public const string ErrorMarker = "+*";
    public const string ErrorMessage = "Syntax error, expected: ...";

    private const string MeasureHeader = "[x] :=\n";
    private const string FunctionHeader = "FUNCTION x =\n";

    public static string Answer(IReadOnlyList<string> dax)
    {
        var results = new JsonArray();
        foreach (var text in dax)
        {
            results.Add(AnswerOne(text));
        }

        return results.ToJsonString();
    }

    private static JsonObject AnswerOne(string text)
    {
        var marker = text.IndexOf(ErrorMarker, StringComparison.Ordinal);
        if (marker >= 0)
        {
            return new JsonObject
            {
                ["formatted"] = string.Empty,
                ["errors"] = new JsonArray(CreateError(text, marker)),
            };
        }

        return new JsonObject
        {
            ["formatted"] = Format(text),
            ["errors"] = new JsonArray(),
        };
    }

    private static JsonObject CreateError(string text, int position)
    {
        var before = text.Substring(0, position);
        var lineStart = before.LastIndexOf('\n') + 1;

        return new JsonObject
        {
            ["line"] = before.Count(character => character == '\n'),
            ["column"] = ToServiceColumn(before.Substring(lineStart)),
            ["message"] = ErrorMessage,
        };
    }

    // The service counts a tab up to the next multiple of 8 and gives a carriage return no width.
    private static int ToServiceColumn(string lineBeforeTheError)
    {
        var column = 0;
        foreach (var character in lineBeforeTheError)
        {
            if (character == '\t')
            {
                column = ((column / 8) + 1) * 8;
            }
            else if (character != '\r')
            {
                column++;
            }
        }

        return column;
    }

    private static string Format(string text)
    {
        if (text.StartsWith(MeasureHeader, StringComparison.Ordinal))
        {
            return "[x] :=\r\n" + ToServiceLines(text.Substring(MeasureHeader.Length), indentation: "") + "\r\n";
        }

        if (!text.StartsWith(FunctionHeader, StringComparison.Ordinal))
        {
            return ToServiceLines(text, indentation: "") + "\r\n";
        }

        // The service puts the parameters on the line of the header, unless a comment comes first.
        var body = ToServiceLines(text.Substring(FunctionHeader.Length), indentation: "    ");
        var startsWithParameters = body.Length > 0 && body[0] == '(';
        return (startsWithParameters ? "FUNCTION x = " : "FUNCTION x =\r\n    ") + body + "\r\n";
    }

    private static string ToServiceLines(string text, string indentation)
    {
        var lines = text.Split('\n').Select(line => line.TrimEnd('\r'));
        return string.Join("\r\n" + indentation, lines);
    }
}
