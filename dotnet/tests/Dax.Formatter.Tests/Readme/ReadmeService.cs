using System.Net.Http;
using System.Text.Json.Nodes;
using Dax.Formatter.Tests.Fakes;

namespace Dax.Formatter.Tests.Readme;

/// <summary>
/// Stands in for the service in the samples of the readme: it gives the answers that the service gave for the
/// texts of the samples, and the answer of <see cref="EchoService"/> for any other text.
/// </summary>
internal static class ReadmeService
{
    private static readonly Dictionary<string, string> s_formatted = new(StringComparer.Ordinal)
    {
        ["EVALUATE FILTER(Customer, Customer[Country] = \"Italy\")"] =
            "EVALUATE\r\nFILTER ( Customer, Customer[Country] = \"Italy\" )\r\n",
        ["[x] :=\nCALCULATE(SUM(Sales[Amount]), Sales[Quantity] > 0)"] =
            "[x] :=\r\nCALCULATE ( SUM ( Sales[Amount] ), Sales[Quantity] > 0 )\r\n",
        ["[x] :=\nDIVIDE([Sales Amount], [Total Cost])"] =
            "[x] :=\r\nDIVIDE ( [Sales Amount], [Total Cost] )\r\n",
        ["FUNCTION x =\n(amount : NUMERIC, rate : NUMERIC) => amount * (1 + rate)"] =
            "FUNCTION x = (\r\n        amount : NUMERIC,\r\n        rate : NUMERIC\r\n    ) =>\r\n    amount * ( 1 + rate )\r\n",
    };

    // The service reports the end of the input as the line after the last one.
    private static readonly Dictionary<string, (int Line, int Column)> s_errors = new(StringComparer.Ordinal)
    {
        ["[x] :=\nSUMX(Sales, Sales[Amount] *"] = (2, 0),
    };

    public static FakeFormatterHandler Handler { get; } = new(Respond.Json(Answer));

    public static HttpClient HttpClient { get; } = new(Handler);

    private static string Answer(IReadOnlyList<string> dax)
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
        if (s_formatted.TryGetValue(text, out var formatted))
        {
            return EchoService.Formatted(formatted);
        }

        if (s_errors.TryGetValue(text, out var position))
        {
            return EchoService.Error(position.Line, position.Column);
        }

        return EchoService.AnswerOne(text);
    }
}
