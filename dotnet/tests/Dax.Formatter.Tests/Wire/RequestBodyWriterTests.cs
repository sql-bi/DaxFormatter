using System.Text.Json;
using Dax.Formatter.Tests.Fakes;
using Dax.Formatter.Tests.Fixtures;
using Xunit;

namespace Dax.Formatter.Tests.Wire;

public class RequestBodyWriterTests
{
    private const string Fixture = "request-bodies.json";

    public static IEnumerable<object[]> Cases => FixtureFile.CaseNames(Fixture);

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task FormatBatchAsync_FixtureCall_SendsTheExpectedBody(string name)
    {
        var testCase = FixtureFile.Case(Fixture, name);
        var application = testCase.GetProperty("application");
        var texts = testCase.GetProperty("texts").EnumerateArray().Select(text => text.GetString()!);
        using var harness = new FormatterHarness(Respond.Echo, options =>
        {
            options.ApplicationName = application.GetProperty("name").GetString();
            options.ApplicationVersion = application.GetProperty("version").GetString();
        });

        await harness.Client
            .FormatBatchAsync(texts, FixtureOptions.Read(testCase.GetProperty("options")))
            .ConfigureAwait(true);

        var request = Assert.Single(harness.Handler.Requests);
        Assert.True(
            JsonElement.DeepEquals(testCase.GetProperty("body"), request.Json),
            $"The body sent is {request.Json.GetRawText()}");
    }

    [Fact]
    public async Task FormatBatchAsync_AnyCall_SendsJsonAsUtf8()
    {
        using var harness = new FormatterHarness(Respond.Echo);

        await harness.Client.FormatAsync("EVALUATE ROW(\"città\", 1)").ConfigureAwait(true);

        var request = Assert.Single(harness.Handler.Requests);
        Assert.Equal("application/json; charset=utf-8", request.ContentType);
        Assert.Equal("EVALUATE ROW(\"città\", 1)", Assert.Single(request.Dax));
    }
}
