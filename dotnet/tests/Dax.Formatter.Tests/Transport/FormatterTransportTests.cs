using System.Net.Http;
using Dax.Formatter.Tests.Fakes;
using Xunit;

namespace Dax.Formatter.Tests.Transport;

public class FormatterTransportTests
{
    [Fact]
    public async Task FormatAsync_AnyCall_PostsToTheFormatEndpoint()
    {
        using var harness = new FormatterHarness(Respond.Echo);

        await harness.Client.FormatAsync("EVALUATE Sales").ConfigureAwait(true);

        var request = Assert.Single(harness.Handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(new Uri("https://api.daxformatter.com/api/daxtextformatmulti"), request.Uri);
    }

    [Fact]
    public async Task FormatAsync_HttpClientWithBaseAddress_IgnoresIt()
    {
        using var harness = new FormatterHarness(
            Respond.Echo,
            options => options.Endpoint = new Uri("https://staging.example.test/dax"));
        harness.HttpClient.BaseAddress = new Uri("https://other.example.test/");

        await harness.Client.FormatAsync("EVALUATE Sales").ConfigureAwait(true);

        var request = Assert.Single(harness.Handler.Requests);
        Assert.Equal(new Uri("https://staging.example.test/dax/api/daxtextformatmulti"), request.Uri);
    }

    [Fact]
    public async Task FormatAsync_AnyCall_IdentifiesTheCallerAndTheRequest()
    {
        using var harness = new FormatterHarness(Respond.Echo);

        await harness.Client.FormatAsync("EVALUATE Sales").ConfigureAwait(true);

        var request = Assert.Single(harness.Handler.Requests);
        Assert.StartsWith("Test-App/1.2.3 Dax.Formatter/", request.GetHeader("User-Agent"), StringComparison.Ordinal);
        Assert.True(Guid.TryParse(request.GetHeader("DaxFormatter-Request-Id"), out _));
        Assert.True(Guid.TryParse(request.GetHeader("DaxFormatter-Operation-Id"), out _));
    }

    [Fact]
    public async Task FormatAsync_AnyCall_AsksForJsonWithoutCompressionOrExpect()
    {
        using var harness = new FormatterHarness(Respond.Echo);

        await harness.Client.FormatAsync("EVALUATE Sales").ConfigureAwait(true);

        var request = Assert.Single(harness.Handler.Requests);
        Assert.Equal("application/json", request.GetHeader("Accept"));
        Assert.Null(request.GetHeader("Accept-Encoding"));
        Assert.False(request.ExpectContinue);
    }

    [Fact]
    public async Task FormatAsync_HttpClientOfTheCaller_LeavesItUnchanged()
    {
        using var harness = new FormatterHarness(Respond.Echo);
        var timeout = harness.HttpClient.Timeout;

        await harness.Client.FormatAsync("EVALUATE Sales").ConfigureAwait(true);

        Assert.Empty(harness.HttpClient.DefaultRequestHeaders);
        Assert.Equal(timeout, harness.HttpClient.Timeout);
    }

    [Fact]
    public async Task FormatAsync_NoAnswerWithinTheRequestTimeout_FailsWithTimeout()
    {
        using var harness = new FormatterHarness(Respond.Hang, options =>
        {
            options.RequestTimeout = TimeSpan.FromMilliseconds(50);
            options.MaxRetries = 0;
        });

        var exception = await Assert
            .ThrowsAsync<DaxFormatterException>(() => harness.Client.FormatAsync("EVALUATE Sales"))
            .ConfigureAwait(true);

        Assert.Equal(DaxFormatterFailureReason.Timeout, exception.Reason);
        Assert.Equal(harness.Handler.Requests[0].GetHeader("DaxFormatter-Request-Id"), exception.RequestId);
        Assert.Equal((null, null), (exception.StatusCode, exception.ServiceRequestId));
    }

    [Fact]
    public async Task FormatAsync_NoAnswerWithinTheRequestTimeout_SaysHowLongItWaited()
    {
        using var harness = new FormatterHarness(Respond.Hang, options =>
        {
            options.RequestTimeout = TimeSpan.FromMilliseconds(50);
            options.MaxRetries = 0;
        });

        var exception = await Assert
            .ThrowsAsync<DaxFormatterException>(() => harness.Client.FormatAsync("EVALUATE Sales"))
            .ConfigureAwait(true);

        Assert.Contains("within 0.05 seconds", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FormatAsync_TimeoutOfTheHttpClientOfTheCaller_FailsWithTimeoutWithoutNamingTheRequestTimeout()
    {
        using var harness = new FormatterHarness(
            Respond.Throw(() => new TaskCanceledException("The request was canceled due to HttpClient.Timeout.")),
            options => options.MaxRetries = 0);

        var exception = await Assert
            .ThrowsAsync<DaxFormatterException>(() => harness.Client.FormatAsync("EVALUATE Sales"))
            .ConfigureAwait(true);

        Assert.Equal(DaxFormatterFailureReason.Timeout, exception.Reason);
        Assert.DoesNotContain("60 seconds", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FormatAsync_RequestThatDoesNotReachTheService_FailsWithNetworkError()
    {
        using var harness = new FormatterHarness(
            Respond.Throw(() => new HttpRequestException("No such host is known.")),
            options => options.MaxRetries = 0);

        var exception = await Assert
            .ThrowsAsync<DaxFormatterException>(() => harness.Client.FormatAsync("EVALUATE Sales"))
            .ConfigureAwait(true);

        Assert.Equal(DaxFormatterFailureReason.NetworkError, exception.Reason);
        Assert.IsType<HttpRequestException>(exception.InnerException);
        Assert.Contains("No such host is known.", exception.Message, StringComparison.Ordinal);
    }
}
