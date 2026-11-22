using System.Net;

namespace Dax.Formatter.Transport;

/// <summary>
/// Decides whether a failed attempt is worth another one, and how long to wait before it.
/// </summary>
internal sealed class RetryPolicy
{
    private static readonly TimeSpan s_firstDelay = TimeSpan.FromSeconds(0.8);
    private static readonly TimeSpan s_longestDelay = TimeSpan.FromSeconds(60);

    private readonly int _maxRetries;
    private readonly TimeSpan _requestTimeout;

    public RetryPolicy(int maxRetries, TimeSpan requestTimeout)
    {
        _maxRetries = maxRetries;
        _requestTimeout = requestTimeout;
    }

    /// <param name="failure">The failed attempt.</param>
    /// <param name="attempt">The number of the failed attempt, from 1.</param>
    /// <param name="requestId">The identifier of the request, which spreads the waits of different requests.</param>
    /// <param name="delay">How long to wait before the next attempt.</param>
    /// <returns><see langword="false"/> when the request must fail now.</returns>
    public bool TryGetDelay(AttemptResult failure, int attempt, Guid requestId, out TimeSpan delay)
    {
        delay = TimeSpan.Zero;
        if (attempt > _maxRetries || !IsTransient(failure.Reason, failure.StatusCode))
        {
            return false;
        }

        delay = GetBackoff(attempt, requestId);
        if (failure.RetryAfter is not TimeSpan retryAfter)
        {
            return true;
        }

        // A service that asks to wait longer than a whole request may last is not coming back soon.
        if (retryAfter > _requestTimeout)
        {
            return false;
        }

        if (retryAfter > delay)
        {
            delay = retryAfter;
        }

        return true;
    }

    public static bool IsTransient(DaxFormatterFailureReason reason, HttpStatusCode? statusCode)
    {
        if (reason is DaxFormatterFailureReason.Timeout or DaxFormatterFailureReason.NetworkError)
        {
            return true;
        }

        if (reason != DaxFormatterFailureReason.HttpError || statusCode is null)
        {
            return false;
        }

        return (int)statusCode.Value is 408 or 429 or 500 or 502 or 503 or 504;
    }

    // Each wait doubles the one before, and varies by a quarter either way so that requests which failed
    // together do not come back together. The variation comes from the request identifier, which is random.
    private static TimeSpan GetBackoff(int attempt, Guid requestId)
    {
        var doublings = Math.Min(attempt - 1, 16);
        var milliseconds = Math.Min(s_firstDelay.TotalMilliseconds * (1 << doublings), s_longestDelay.TotalMilliseconds);

        var bytes = requestId.ToByteArray();
        var fraction = bytes[attempt % bytes.Length] / 255.0;

        return TimeSpan.FromMilliseconds(milliseconds * (0.75 + (0.5 * fraction)));
    }
}
