using System.Net;

namespace Dax.Formatter.Transport;

/// <summary>
/// One request to the service: its body, which every attempt sends again, the identifiers it travels with, and
/// which texts of the call it carries.
/// </summary>
internal sealed class FormatRequest
{
    public FormatRequest(byte[] body, int textCount, int firstIndex, int lastIndex, Guid operationId)
    {
        Body = body;
        TextCount = textCount;
        FirstIndex = firstIndex;
        LastIndex = lastIndex;
        OperationId = operationId;
        RequestId = Guid.NewGuid();
    }

    public byte[] Body { get; }

    public int TextCount { get; }

    /// <summary>Gets the index, in the texts of the call, of the first text of the request.</summary>
    public int FirstIndex { get; }

    /// <summary>Gets the index, in the texts of the call, of the last text of the request.</summary>
    public int LastIndex { get; }

    /// <summary>Gets the identifier of the call, which its requests share.</summary>
    public Guid OperationId { get; }

    /// <summary>Gets the identifier of the request, which its attempts share.</summary>
    public Guid RequestId { get; }

    public DaxFormatterException CreateException(
        DaxFormatterFailureReason reason,
        string detail,
        HttpStatusCode? statusCode,
        string? serviceRequestId,
        Exception? innerException)
    {
        var texts = FirstIndex == LastIndex
            ? FormattableString.Invariant($"the text at index {FirstIndex}")
            : FormattableString.Invariant($"the texts at indexes {FirstIndex} to {LastIndex}");
        var message = $"The DAX Formatter request for {texts} failed: {detail}.";

        return new DaxFormatterException(
            message,
            reason,
            statusCode,
            RequestId.ToString(),
            serviceRequestId,
            innerException);
    }
}
