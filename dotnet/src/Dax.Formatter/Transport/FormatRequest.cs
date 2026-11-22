using System.Net;

namespace Dax.Formatter.Transport;

/// <summary>
/// One request to the service: its body, which every attempt sends again, the identifiers it travels with, and
/// which texts of the call it carries.
/// </summary>
internal sealed class FormatRequest
{
    public FormatRequest(byte[] body, IReadOnlyList<int> textIndexes, Guid operationId)
    {
        Body = body;
        TextIndexes = textIndexes;
        OperationId = operationId;
        RequestId = Guid.NewGuid();
    }

    public byte[] Body { get; }

    public int TextCount => TextIndexes.Count;

    /// <summary>Gets the indexes, in the texts of the call, of the texts of the request, in order.</summary>
    public IReadOnlyList<int> TextIndexes { get; }

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
        var first = TextIndexes[0];
        var last = TextIndexes[TextIndexes.Count - 1];
        var texts = first == last
            ? FormattableString.Invariant($"the text at index {first}")
            : FormattableString.Invariant($"the texts at indexes {first} to {last}");
        var message = $"The DAX Formatter request for {texts} failed: {detail}.";

        return new DaxFormatterException(
            message,
            reason,
            statusCode,
            RequestId.ToString(),
            serviceRequestId,
            OperationId.ToString(),
            TextIndexes,
            innerException);
    }
}
