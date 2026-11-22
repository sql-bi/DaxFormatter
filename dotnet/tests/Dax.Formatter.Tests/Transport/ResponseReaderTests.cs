using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dax.Formatter.Transport;
using Xunit;

namespace Dax.Formatter.Tests.Transport;

public class ResponseReaderTests
{
    private const string TwoResults =
        "[{\"formatted\":\"a\\r\\n\",\"errors\":[]},{\"formatted\":\"\",\"errors\":[{\"line\":0,\"column\":1,\"message\":\"m\"}]}]";

    [Fact]
    public async Task ReadAsync_OneResultForEachText_ReturnsThem()
    {
        using var response = CreateResponse(HttpStatusCode.OK, TwoResults, "application/json");

        var result = await ResponseReader.ReadAsync(response, 2, CancellationToken.None).ConfigureAwait(true);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Items!.Count);
        Assert.Equal("ref-1", result.ServiceRequestId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task ReadAsync_ResultCountDifferentFromTheTexts_IsAnInvalidResponse(int expectedCount)
    {
        using var response = CreateResponse(HttpStatusCode.OK, TwoResults, "application/json");

        var result = await ResponseReader.ReadAsync(response, expectedCount, CancellationToken.None).ConfigureAwait(true);

        Assert.Equal(DaxFormatterFailureReason.InvalidResponse, result.Reason);
        Assert.Contains("2 results", result.Detail, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("<html><body>Service unavailable</body></html>", "text/html")]
    [InlineData("", "application/json")]
    [InlineData("{\"formatted\":\"a\"}", "application/json")]
    [InlineData("[{\"formatted\":1}]", "application/json")]
    [InlineData("[{\"formatted\":\"a\"", "application/json")]
    public async Task ReadAsync_BodyThatIsNotTheDocumentedJson_IsAnInvalidResponse(string body, string mediaType)
    {
        using var response = CreateResponse(HttpStatusCode.OK, body, mediaType);

        var result = await ResponseReader.ReadAsync(response, 1, CancellationToken.None).ConfigureAwait(true);

        Assert.Equal(DaxFormatterFailureReason.InvalidResponse, result.Reason);
        Assert.IsType<JsonException>(result.Error, exactMatch: false);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("[null]")]
    public async Task ReadAsync_NullInPlaceOfResults_IsAnInvalidResponse(string body)
    {
        using var response = CreateResponse(HttpStatusCode.OK, body, "application/json");

        var result = await ResponseReader.ReadAsync(response, 1, CancellationToken.None).ConfigureAwait(true);

        Assert.Equal(DaxFormatterFailureReason.InvalidResponse, result.Reason);
    }

    [Fact]
    public async Task ReadAsync_NoContentStatus_IsAnInvalidResponse()
    {
        using var response = CreateResponse(HttpStatusCode.NoContent, string.Empty, "application/json");

        var result = await ResponseReader.ReadAsync(response, 1, CancellationToken.None).ConfigureAwait(true);

        Assert.Equal(DaxFormatterFailureReason.InvalidResponse, result.Reason);
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }

    [Fact]
    public async Task ReadAsync_StatusThatIsNotASuccess_IsAnHttpErrorWithItsDetails()
    {
        using var response = CreateResponse(HttpStatusCode.ServiceUnavailable, "busy", "text/plain");
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(3));

        var result = await ResponseReader.ReadAsync(response, 1, CancellationToken.None).ConfigureAwait(true);

        Assert.Equal(DaxFormatterFailureReason.HttpError, result.Reason);
        Assert.Equal((HttpStatusCode.ServiceUnavailable, "ref-1"), (result.StatusCode, result.ServiceRequestId));
        Assert.Equal(TimeSpan.FromSeconds(3), result.RetryAfter);
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode status, string body, string mediaType)
    {
        var response = new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, mediaType),
        };
        response.Headers.Add("x-azure-ref", "ref-1");
        return response;
    }
}
