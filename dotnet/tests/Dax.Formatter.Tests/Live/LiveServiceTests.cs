using System.Text.Json;
using Dax.Formatter.Tests.Fixtures;
using Xunit;

namespace Dax.Formatter.Tests.Live;

/// <summary>
/// Pins, on the live service, the facts that the client relies on. A failure here with green offline tests
/// means that the service changed: the fixtures and the client follow.
/// </summary>
[Collection(LiveServiceTestGroup.Name)]
[Trait("Category", "Live")]
public class LiveServiceTests
{
    private const string Fixture = "format-results.json";

    private static readonly DaxFormatterClient s_client = new(new DaxFormatterClientOptions
    {
        ApplicationName = "Dax.Formatter.Tests",
        ApplicationVersion = "2.0",
    });

    private static readonly DaxFormatOptions s_expression = new() { Kind = DaxKind.Expression };

    [LiveFact]
    public async Task FormatBatchAsync_FixtureTextsSentAsTheyAre_ReturnsTheRecordedResults()
    {
        await AssertFixtureResultsAsync(DaxKind.Auto).ConfigureAwait(true);
    }

    [LiveFact]
    public async Task FormatBatchAsync_FixtureExpressions_ReturnsTheRecordedResults()
    {
        await AssertFixtureResultsAsync(DaxKind.Expression).ConfigureAwait(true);
    }

    [LiveFact]
    public async Task FormatAsync_FunctionBody_ReturnsItFormattedWithoutHeaderAndIndentation()
    {
        var result = await s_client
            .FormatAsync("// tax\n(amount : NUMERIC, rate : NUMERIC) => amount * (1 + rate)", s_expression)
            .ConfigureAwait(true);

        Assert.Equal(
            "// tax\r\n(\r\n    amount : NUMERIC,\r\n    rate : NUMERIC\r\n) =>\r\namount * ( 1 + rate )",
            result.FormattedText);
    }

    [LiveFact]
    public async Task FormatBatchAsync_KnownLimitsOfTheService_StillReturnSyntaxErrors()
    {
        string[] texts = ["() => 1", "(a, b = 1) => a + b", "\"a => b\""];

        var results = await s_client.FormatBatchAsync(texts, s_expression).ConfigureAwait(true);

        // When one of these starts to succeed, the service has lost that limit: update the documents.
        Assert.Equal([false, false, false], results.Select(result => result.Succeeded));
        Assert.Equal((0, 0), (results[2].Errors[0].Line, results[2].Errors[0].Column));
    }

    [LiveFact]
    public async Task FormatBatchAsync_MoreTextsThanARequestCarries_ReturnsEveryResultInOrder()
    {
        var texts = Enumerable.Range(1, 600).Select(number => "[Sales]*" + number).ToList();

        var results = await s_client.FormatBatchAsync(texts, s_expression).ConfigureAwait(true);

        var expected = Enumerable.Range(1, 600).Select(number => "[Sales] * " + number);
        Assert.Equal(expected, results.Select(result => result.FormattedText));
    }

    [LiveFact]
    public async Task FormatAsync_ShortLinesWithoutSpaceAfterFunctions_FormatsThatWay()
    {
        var options = new DaxFormatOptions
        {
            Kind = DaxKind.Expression,
            LineStyle = DaxLineStyle.ShortLine,
            SpacingStyle = DaxSpacingStyle.NoSpaceAfterFunction,
        };

        var result = await s_client.FormatAsync("IF(1=1,1,0)", options).ConfigureAwait(true);

        Assert.Equal("IF(\r\n    1 = 1,\r\n    1,\r\n    0\r\n)", result.FormattedText);
    }

    [LiveFact]
    public async Task FormatAsync_SemicolonAndCommaWithEveryModelProperty_FormatsTheText()
    {
        var options = new DaxFormatOptions
        {
            Kind = DaxKind.Expression,
            SeparatorStyle = DaxSeparatorStyle.SemicolonAndComma,
            ModelInfo = new DaxModelInfo
            {
                ServerName = "MyServerName1",
                DatabaseName = "MyDatabaseName1",
                ServerType = DaxServerTypes.AnalysisServices,
                ServerMode = "Tabular",
                ServerEdition = "Enterprise64",
                ServerLocation = "OnPremise",
                ServerVersion = "16.0.143.21",
                CompatibilityLevel = 1600,
            },
        };

        var result = await s_client.FormatAsync("DIVIDE(1,5;2)", options).ConfigureAwait(true);

        Assert.Equal("DIVIDE ( 1,5; 2 )", result.FormattedText);
    }

    private static async Task AssertFixtureResultsAsync(DaxKind kind)
    {
        var cases = FixtureFile.Cases(Fixture)
            .Where(testCase => FixtureOptions.ReadKind(testCase) == kind)
            .ToList();
        var texts = cases.Select(testCase => testCase.GetProperty("text").GetString()!);

        var results = await s_client
            .FormatBatchAsync(texts, new DaxFormatOptions { Kind = kind })
            .ConfigureAwait(true);

        Assert.All(cases.Zip(results, (testCase, result) => (testCase, result)), pair =>
        {
            var name = pair.testCase.GetProperty("name").GetString();
            Assert.True(Describe(pair.testCase.GetProperty("result")) == Describe(pair.result), name);
        });
    }

    // The message of an error is long and lists what the grammar accepts, so only its position is compared.
    private static string Describe(JsonElement expected)
    {
        if (expected.TryGetProperty("formattedText", out var formattedText))
        {
            return "text: " + formattedText.GetString();
        }

        var positions = expected.GetProperty("errors").EnumerateArray().Select(
            error => (error.GetProperty("line").GetInt32(), error.GetProperty("column").GetInt32()));
        return "errors: " + string.Join(" ", positions);
    }

    private static string Describe(DaxFormatResult result)
    {
        return result.Succeeded
            ? "text: " + result.FormattedText
            : "errors: " + string.Join(" ", result.Errors.Select(error => (error.Line, error.Column)));
    }
}
