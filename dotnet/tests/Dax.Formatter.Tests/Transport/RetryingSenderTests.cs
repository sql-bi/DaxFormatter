using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Dax.Formatter.Tests.Fakes;
using Xunit;

namespace Dax.Formatter.Tests.Transport;

public class RetryingSenderTests
{
    private static readonly Responder s_busy = Respond.Status(HttpStatusCode.ServiceUnavailable);

    [Fact]
    public async Task FormatAsync_TransientFailureThenSuccess_ReturnsTheResult()
    {
        using var harness = new FormatterHarness(Respond.Sequence(s_busy, Respond.Echo));

        var result = await harness.Client.FormatAsync("EVALUATE Sales").ConfigureAwait(true);

        Assert.True(result.Succeeded);
        Assert.Equal(2, harness.Handler.Requests.Count);
        Assert.Single(harness.Delay.Delays);
    }

    [Fact]
    public async Task FormatAsync_SecondAttempt_SendsTheSameBodyAndRequestId()
    {
        using var harness = new FormatterHarness(Respond.Sequence(s_busy, Respond.Echo));

        await harness.Client.FormatAsync("EVALUATE Sales").ConfigureAwait(true);

        var first = harness.Handler.Requests[0];
        var second = harness.Handler.Requests[1];
        Assert.Equal(first.Body, second.Body);
        Assert.Equal(first.GetHeader("DaxFormatter-Request-Id"), second.GetHeader("DaxFormatter-Request-Id"));
    }

    [Fact]
    public async Task FormatAsync_NetworkErrorThenSuccess_ReturnsTheResult()
    {
        var unreachable = Respond.Throw(() => new HttpRequestException("Connection reset."));
        using var harness = new FormatterHarness(Respond.Sequence(unreachable, Respond.Echo));

        var result = await harness.Client.FormatAsync("EVALUATE Sales").ConfigureAwait(true);

        Assert.True(result.Succeeded);
        Assert.Equal(2, harness.Handler.Requests.Count);
    }

    [Fact]
    public async Task FormatAsync_TransientFailureEveryTime_FailsAfterTheRetries()
    {
        using var harness = new FormatterHarness(s_busy, options => options.MaxRetries = 2);

        var exception = await Assert
            .ThrowsAsync<DaxFormatterException>(() => harness.Client.FormatAsync("EVALUATE Sales"))
            .ConfigureAwait(true);

        Assert.Equal(3, harness.Handler.Requests.Count);
        Assert.Equal(2, harness.Delay.Delays.Count);
        Assert.Contains("503", exception.Message, StringComparison.Ordinal);
        Assert.Contains("after 3 attempts", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FormatAsync_NoRetriesAllowed_FailsAtTheFirstFailure()
    {
        using var harness = new FormatterHarness(s_busy, options => options.MaxRetries = 0);

        await Assert
            .ThrowsAsync<DaxFormatterException>(() => harness.Client.FormatAsync("EVALUATE Sales"))
            .ConfigureAwait(true);

        Assert.Single(harness.Handler.Requests);
        Assert.Empty(harness.Delay.Delays);
    }

    [Fact]
    public async Task FormatAsync_StatusThatIsNotTransient_FailsAtOnceWithItsDetails()
    {
        using var harness = new FormatterHarness(Respond.Status(HttpStatusCode.BadRequest));

        var exception = await Assert
            .ThrowsAsync<DaxFormatterException>(() => harness.Client.FormatAsync("EVALUATE Sales"))
            .ConfigureAwait(true);

        Assert.Single(harness.Handler.Requests);
        Assert.Equal(DaxFormatterFailureReason.HttpError, exception.Reason);
        Assert.Equal((HttpStatusCode.BadRequest, Respond.ServiceRequestId), (exception.StatusCode, exception.ServiceRequestId));
    }

    [Fact]
    public async Task FormatAsync_InvalidResponse_FailsAtOnce()
    {
        var html = Respond.Content(HttpStatusCode.OK, "<html><body>Sign in</body></html>", "text/html");
        using var harness = new FormatterHarness(html);

        var exception = await Assert
            .ThrowsAsync<DaxFormatterException>(() => harness.Client.FormatAsync("EVALUATE Sales"))
            .ConfigureAwait(true);

        Assert.Single(harness.Handler.Requests);
        Assert.Equal(DaxFormatterFailureReason.InvalidResponse, exception.Reason);
    }

    [Fact]
    public async Task FormatAsync_RetryAfter_WaitsAsLongAsTheServiceAsks()
    {
        var throttled = Respond.Status(
            (HttpStatusCode)429,
            response => response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(20)));
        using var harness = new FormatterHarness(Respond.Sequence(throttled, Respond.Echo));

        await harness.Client.FormatAsync("EVALUATE Sales").ConfigureAwait(true);

        Assert.Equal(TimeSpan.FromSeconds(20), Assert.Single(harness.Delay.Delays));
    }

    [Fact]
    public async Task FormatAsync_RetryAfterLongerThanTheRequestTimeout_FailsAtOnce()
    {
        var throttled = Respond.Status(
            (HttpStatusCode)429,
            response => response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(120)));
        using var harness = new FormatterHarness(throttled);

        var exception = await Assert
            .ThrowsAsync<DaxFormatterException>(() => harness.Client.FormatAsync("EVALUATE Sales"))
            .ConfigureAwait(true);

        Assert.Single(harness.Handler.Requests);
        Assert.Equal((HttpStatusCode)429, exception.StatusCode);
    }

    [Fact]
    public async Task FormatAsync_CancellationDuringTheWait_ThrowsOperationCanceled()
    {
        using var cancellation = new CancellationTokenSource();
        using var harness = new FormatterHarness(s_busy);
        harness.Delay.OnWait = cancellation.Cancel;

        var exception = await Assert
            .ThrowsAnyAsync<OperationCanceledException>(
                () => harness.Client.FormatAsync("EVALUATE Sales", cancellationToken: cancellation.Token))
            .ConfigureAwait(true);

        Assert.Equal(cancellation.Token, exception.CancellationToken);
        Assert.Single(harness.Handler.Requests);
    }
}
