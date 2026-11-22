using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Dax.Formatter.Transport;
using Xunit;

namespace Dax.Formatter.Tests.Transport;

public class RetryPolicyTests
{
    private static readonly Guid s_requestId = new("0b7f3c1e-2a4d-4e6f-8a9b-1c2d3e4f5a6b");
    private static readonly TimeSpan s_requestTimeout = TimeSpan.FromSeconds(60);

    [Theory]
    [InlineData(DaxFormatterFailureReason.Timeout, null, true)]
    [InlineData(DaxFormatterFailureReason.NetworkError, null, true)]
    [InlineData(DaxFormatterFailureReason.HttpError, 408, true)]
    [InlineData(DaxFormatterFailureReason.HttpError, 429, true)]
    [InlineData(DaxFormatterFailureReason.HttpError, 500, true)]
    [InlineData(DaxFormatterFailureReason.HttpError, 502, true)]
    [InlineData(DaxFormatterFailureReason.HttpError, 503, true)]
    [InlineData(DaxFormatterFailureReason.HttpError, 504, true)]
    [InlineData(DaxFormatterFailureReason.HttpError, 400, false)]
    [InlineData(DaxFormatterFailureReason.HttpError, 401, false)]
    [InlineData(DaxFormatterFailureReason.HttpError, 403, false)]
    [InlineData(DaxFormatterFailureReason.HttpError, 404, false)]
    [InlineData(DaxFormatterFailureReason.HttpError, 413, false)]
    [InlineData(DaxFormatterFailureReason.HttpError, 501, false)]
    [InlineData(DaxFormatterFailureReason.HttpError, null, false)]
    [InlineData(DaxFormatterFailureReason.InvalidResponse, 200, false)]
    [InlineData(DaxFormatterFailureReason.InvalidResponse, 204, false)]
    [InlineData(DaxFormatterFailureReason.Unknown, 503, false)]
    public void IsTransient_ReasonAndStatus_SaysWhetherAnotherAttemptCanHelp(
        DaxFormatterFailureReason reason, int? status, bool expected)
    {
        var transient = RetryPolicy.IsTransient(reason, (HttpStatusCode?)status);

        Assert.Equal(expected, transient);
    }

    [Theory]
    [InlineData(1, 600, 1000)]
    [InlineData(2, 1200, 2000)]
    [InlineData(3, 2400, 4000)]
    [InlineData(40, 45000, 75000)]
    public void TryGetDelay_TransientFailure_DoublesTheWaitWithinAQuarter(
        int attempt, int minMilliseconds, int maxMilliseconds)
    {
        var policy = new RetryPolicy(maxRetries: 100, s_requestTimeout);

        var retry = policy.TryGetDelay(Timeout(), attempt, s_requestId, out var delay);

        Assert.True(retry);
        Assert.InRange(delay.TotalMilliseconds, minMilliseconds, maxMilliseconds);
    }

    [Fact]
    public void TryGetDelay_DifferentRequests_SpreadsTheWaits()
    {
        var policy = new RetryPolicy(maxRetries: 2, s_requestTimeout);

        policy.TryGetDelay(Timeout(), 1, new Guid("00000000-0000-0000-0000-000000000000"), out var shortest);
        policy.TryGetDelay(Timeout(), 1, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), out var longest);

        Assert.Equal(TimeSpan.FromMilliseconds(600), shortest);
        Assert.Equal(TimeSpan.FromMilliseconds(1000), longest);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(2, 3)]
    public void TryGetDelay_AttemptsUsedUp_ReturnsFalse(int maxRetries, int attempt)
    {
        var policy = new RetryPolicy(maxRetries, s_requestTimeout);

        var retry = policy.TryGetDelay(Timeout(), attempt, s_requestId, out _);

        Assert.False(retry);
    }

    [Fact]
    public void TryGetDelay_FailureThatIsNotTransient_ReturnsFalse()
    {
        var policy = new RetryPolicy(maxRetries: 2, s_requestTimeout);
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var failure = AttemptResult.Rejected(DaxFormatterFailureReason.HttpError, "detail", response);

        var retry = policy.TryGetDelay(failure, 1, s_requestId, out _);

        Assert.False(retry);
    }

    [Theory]
    [InlineData(30, 30000)]
    [InlineData(60, 60000)]
    public void TryGetDelay_RetryAfterLongerThanTheBackoff_WaitsAsLongAsAsked(int seconds, int expectedMilliseconds)
    {
        var policy = new RetryPolicy(maxRetries: 2, s_requestTimeout);
        var failure = Busy(retryAfterSeconds: seconds);

        var retry = policy.TryGetDelay(failure, 1, s_requestId, out var delay);

        Assert.True(retry);
        Assert.Equal(TimeSpan.FromMilliseconds(expectedMilliseconds), delay);
    }

    [Fact]
    public void TryGetDelay_RetryAfterShorterThanTheBackoff_KeepsTheBackoff()
    {
        var policy = new RetryPolicy(maxRetries: 2, s_requestTimeout);
        var failure = Busy(retryAfterSeconds: 0);

        policy.TryGetDelay(failure, 1, s_requestId, out var delay);

        Assert.InRange(delay.TotalMilliseconds, 600, 1000);
    }

    [Fact]
    public void TryGetDelay_RetryAfterLongerThanTheRequestTimeout_ReturnsFalse()
    {
        var policy = new RetryPolicy(maxRetries: 2, s_requestTimeout);
        var failure = Busy(retryAfterSeconds: 61);

        var retry = policy.TryGetDelay(failure, 1, s_requestId, out _);

        Assert.False(retry);
    }

    private static AttemptResult Timeout()
    {
        return AttemptResult.NoResponse(DaxFormatterFailureReason.Timeout, "detail", error: null);
    }

    private static AttemptResult Busy(int retryAfterSeconds)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(retryAfterSeconds));

        return AttemptResult.Rejected(DaxFormatterFailureReason.HttpError, "detail", response);
    }
}
