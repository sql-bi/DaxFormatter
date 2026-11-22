namespace Dax.Formatter;

/// <summary>
/// Why a call to the DAX Formatter service failed.
/// </summary>
public enum DaxFormatterFailureReason
{
    /// <summary>
    /// The reason is not known.
    /// </summary>
    Unknown,

    /// <summary>
    /// The service did not answer within the timeout of a request.
    /// </summary>
    Timeout,

    /// <summary>
    /// The request did not reach the service, or its answer did not arrive.
    /// </summary>
    NetworkError,

    /// <summary>
    /// The service answered with an HTTP status that is not a success.
    /// </summary>
    HttpError,

    /// <summary>
    /// The service answered with something the client cannot use as the results of the request.
    /// </summary>
    InvalidResponse,
}
