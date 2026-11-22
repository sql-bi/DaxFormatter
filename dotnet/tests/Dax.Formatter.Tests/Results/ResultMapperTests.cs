using System.Text.Json;
using Dax.Formatter.Expressions;
using Dax.Formatter.Results;
using Dax.Formatter.Tests.Fixtures;
using Dax.Formatter.Wire;
using Xunit;

namespace Dax.Formatter.Tests.Results;

public class ResultMapperTests
{
    private const string Fixture = "format-results.json";

    public static IEnumerable<object[]> Cases => FixtureFile.CaseNames(Fixture);

    [Theory]
    [MemberData(nameof(Cases))]
    public void TryMap_FixtureAnswer_ReturnsTheExpectedResult(string name)
    {
        var testCase = FixtureFile.Case(Fixture, name);
        var text = testCase.GetProperty("text").GetString()!;
        var header = FixtureOptions.ReadKind(testCase) == DaxKind.Expression ? ExpressionHeader.For(text) : null;
        var item = ReadItem(testCase.GetProperty("response").GetRawText());

        var mapped = ResultMapper.TryMap(text, header, item, out var result);

        Assert.True(mapped);
        AssertMatches(testCase.GetProperty("result"), result!);
    }

    [Theory]
    [InlineData("{\"formatted\":\"EVALUATE\\r\\nSales\\r\\n\",\"errors\":null}", "EVALUATE\r\nSales")]
    [InlineData("{\"formatted\":\"EVALUATE\\r\\nSales\\r\\n\"}", "EVALUATE\r\nSales")]
    [InlineData("{\"formatted\":null,\"errors\":[]}", "")]
    [InlineData("{}", "")]
    [InlineData("{\"Formatted\":\"a\\n\",\"Errors\":[]}", "a")]
    [InlineData("{\"formatted\":\"a\\r\\n\\r\\n\",\"errors\":[]}", "a\r\n")]
    public void TryMap_FieldsMissingOrNull_TreatsThemAsEmpty(string json, string expectedText)
    {
        var mapped = ResultMapper.TryMap("EVALUATE Sales", header: null, ReadItem(json), out var result);

        Assert.True(mapped);
        Assert.Equal(expectedText, result!.FormattedText);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void TryMap_ErrorsWithFormattedText_IgnoresTheFormattedText()
    {
        var item = ReadItem("{\"formatted\":\"not a header\",\"errors\":[{\"line\":1,\"column\":2,\"message\":\"m\"}]}");

        var mapped = ResultMapper.TryMap("1 +* 2", ExpressionHeader.Measure, item, out var result);

        Assert.True(mapped);
        Assert.Null(result!.FormattedText);
        Assert.Equal(new DaxSyntaxError(0, 2, "m"), Assert.Single(result.Errors));
    }

    [Theory]
    [InlineData("{\"errors\":[null]}")]
    [InlineData("{\"errors\":[{}]}")]
    [InlineData("{\"errors\":[{\"line\":null,\"column\":null,\"message\":null}]}")]
    public void TryMap_ErrorWithoutFields_PointsToTheStartWithAnEmptyMessage(string json)
    {
        var mapped = ResultMapper.TryMap("1 + 2", header: null, ReadItem(json), out var result);

        Assert.True(mapped);
        Assert.Equal(new DaxSyntaxError(0, 0, string.Empty), Assert.Single(result!.Errors));
    }

    [Theory]
    [InlineData("{\"formatted\":\"1 + 2\\r\\n\",\"errors\":[]}")]
    [InlineData("{\"formatted\":\"\",\"errors\":[]}")]
    [InlineData("{\"formatted\":null}")]
    public void TryMap_FormattedTextWithoutTheHeader_ReturnsFalse(string json)
    {
        var mapped = ResultMapper.TryMap("1 + 2", ExpressionHeader.Measure, ReadItem(json), out var result);

        Assert.False(mapped);
        Assert.Null(result);
    }

    private static FormatResponseItem ReadItem(string json)
    {
        var items = JsonSerializer.Deserialize("[" + json + "]", WireJsonContext.Default.ListFormatResponseItem);
        return items![0]!;
    }

    private static void AssertMatches(JsonElement expected, DaxFormatResult result)
    {
        if (expected.TryGetProperty("formattedText", out var formattedText))
        {
            Assert.Equal(formattedText.GetString(), result.FormattedText);
            Assert.Empty(result.Errors);
            return;
        }

        var errors = expected.GetProperty("errors").EnumerateArray().Select(error => new DaxSyntaxError(
            error.GetProperty("line").GetInt32(),
            error.GetProperty("column").GetInt32(),
            error.GetProperty("message").GetString()!));

        Assert.False(result.Succeeded);
        Assert.Equal(errors, result.Errors);
    }
}
