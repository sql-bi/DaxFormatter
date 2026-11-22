namespace Dax.Formatter.Transport;

/// <summary>
/// Reads the response headers that the client uses.
/// </summary>
internal static class ResponseHeaders
{
    private const string ServiceRequestIdHeader = "x-azure-ref";

    public static string? GetServiceRequestId(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues(ServiceRequestIdHeader, out var values))
        {
            return null;
        }

        return values.FirstOrDefault();
    }

    /// <summary>
    /// Gets the wait that <c>Retry-After</c> asks for, as seconds or as a date. A date is measured from the
    /// <c>Date</c> header of the same response, so that the clock of this machine does not matter.
    /// </summary>
    public static TimeSpan? GetRetryAfter(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter is null)
        {
            return null;
        }

        if (retryAfter.Delta is TimeSpan delta)
        {
            return delta < TimeSpan.Zero ? TimeSpan.Zero : delta;
        }

        if (retryAfter.Date is DateTimeOffset date && response.Headers.Date is DateTimeOffset now)
        {
            return date > now ? date - now : TimeSpan.Zero;
        }

        return null;
    }
}
