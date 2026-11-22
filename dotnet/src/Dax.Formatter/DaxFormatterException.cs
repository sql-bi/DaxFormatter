using System.Net;

namespace Dax.Formatter;

/// <summary>
/// The exception thrown when a call to the DAX Formatter service fails. A syntax error in a DAX text is not a
/// failure: it is part of the <see cref="DaxFormatResult"/> of that text.
/// </summary>
public sealed class DaxFormatterException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DaxFormatterException"/> class.
    /// </summary>
    public DaxFormatterException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DaxFormatterException"/> class with a message.
    /// </summary>
    /// <param name="message">The message that describes the failure.</param>
    public DaxFormatterException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DaxFormatterException"/> class with a message and the
    /// exception that caused the failure.
    /// </summary>
    /// <param name="message">The message that describes the failure.</param>
    /// <param name="innerException">The exception that caused the failure.</param>
    public DaxFormatterException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DaxFormatterException"/> class with every detail of the
    /// failure. The client uses it; it is public so that the tests of an application can create the exception.
    /// </summary>
    /// <param name="message">The message that describes the failure.</param>
    /// <param name="reason">Why the call failed.</param>
    /// <param name="statusCode">The HTTP status of the answer, when one arrived.</param>
    /// <param name="requestId">The identifier that the client gave to the request.</param>
    /// <param name="serviceRequestId">The identifier that the service gave to the request.</param>
    /// <param name="innerException">The exception that caused the failure, if any.</param>
    public DaxFormatterException(
        string message,
        DaxFormatterFailureReason reason,
        HttpStatusCode? statusCode,
        string? requestId,
        string? serviceRequestId,
        Exception? innerException)
        : base(message, innerException)
    {
        Reason = reason;
        StatusCode = statusCode;
        RequestId = requestId;
        ServiceRequestId = serviceRequestId;
    }

    /// <summary>
    /// Gets why the call failed.
    /// </summary>
    public DaxFormatterFailureReason Reason { get; }

    /// <summary>
    /// Gets the HTTP status of the answer, or <see langword="null"/> when no answer arrived.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Gets the identifier that the client gave to the failed request and sent in the
    /// <c>DaxFormatter-Request-Id</c> header. It is set also when no answer arrived.
    /// </summary>
    public string? RequestId { get; }

    /// <summary>
    /// Gets the identifier that the service gave to the failed request, from the <c>x-azure-ref</c> header of the
    /// answer, or <see langword="null"/> when no answer arrived.
    /// </summary>
    public string? ServiceRequestId { get; }
}
