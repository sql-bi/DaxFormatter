using System.Diagnostics.CodeAnalysis;
using System.Net;
using Dax.Formatter.Wire;

namespace Dax.Formatter.Transport;

/// <summary>
/// The outcome of one attempt to send a request: the results of its texts, or the failure with what is needed
/// to decide whether to try again.
/// </summary>
internal sealed class AttemptResult
{
    private AttemptResult(List<FormatResponseItem>? items, DaxFormatterFailureReason reason, string? detail)
    {
        Items = items;
        Reason = reason;
        Detail = detail;
    }

    public List<FormatResponseItem>? Items { get; }

    [MemberNotNullWhen(true, nameof(Items))]
    [MemberNotNullWhen(false, nameof(Detail))]
    public bool Succeeded => Items is not null;

    public DaxFormatterFailureReason Reason { get; }

    /// <summary>Gets the part of the exception message that says what went wrong.</summary>
    public string? Detail { get; }

    public HttpStatusCode? StatusCode { get; private set; }

    /// <summary>Gets the identifier that the service gave to the request, when an answer arrived.</summary>
    public string? ServiceRequestId { get; private set; }

    /// <summary>Gets how long the service asked to wait before the next attempt.</summary>
    public TimeSpan? RetryAfter { get; private set; }

    public Exception? Error { get; private set; }

    public static AttemptResult Success(List<FormatResponseItem> items, HttpResponseMessage response)
    {
        return new AttemptResult(items, DaxFormatterFailureReason.Unknown, detail: null)
        {
            StatusCode = response.StatusCode,
            ServiceRequestId = ResponseHeaders.GetServiceRequestId(response),
        };
    }

    public static AttemptResult NoResponse(DaxFormatterFailureReason reason, string detail, Exception? error)
    {
        return new AttemptResult(items: null, reason, detail)
        {
            Error = error,
        };
    }

    public static AttemptResult Rejected(
        DaxFormatterFailureReason reason,
        string detail,
        HttpResponseMessage response,
        Exception? error = null)
    {
        return new AttemptResult(items: null, reason, detail)
        {
            StatusCode = response.StatusCode,
            ServiceRequestId = ResponseHeaders.GetServiceRequestId(response),
            RetryAfter = ResponseHeaders.GetRetryAfter(response),
            Error = error,
        };
    }
}
