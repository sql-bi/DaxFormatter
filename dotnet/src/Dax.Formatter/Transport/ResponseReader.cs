using System.Net;
using System.Text.Json;
using Dax.Formatter.Wire;

namespace Dax.Formatter.Transport;

/// <summary>
/// Reads the response to a request and checks that it holds one result for each text sent.
/// </summary>
internal static class ResponseReader
{
    public static async Task<AttemptResult> ReadAsync(
        HttpResponseMessage response,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        var status = (int)response.StatusCode;
        if (!response.IsSuccessStatusCode)
        {
            var detail = FormattableString.Invariant($"the service answered {status} ({response.ReasonPhrase})");
            return AttemptResult.Rejected(DaxFormatterFailureReason.HttpError, detail, response);
        }

        // The service answers 204 to a request without texts, which the client never sends.
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return Invalid("the service answered 204 without results", response);
        }

        var content = await ReadContentAsync(response, cancellationToken).ConfigureAwait(false);

        List<FormatResponseItem?>? items;
        try
        {
            items = JsonSerializer.Deserialize(content, WireJsonContext.Default.ListFormatResponseItem);
        }
        catch (JsonException exception)
        {
            return Invalid("the response is not the JSON that the service documents", response, exception);
        }

        if (items is null)
        {
            return Invalid("the response has no results", response);
        }

        if (items.Count != expectedCount)
        {
            var detail = FormattableString.Invariant(
                $"the response has {items.Count} results for {expectedCount} texts");
            return Invalid(detail, response);
        }

        if (items.Contains(null))
        {
            return Invalid("the response has an empty result", response);
        }

        return AttemptResult.Success(items!, response);
    }

    private static AttemptResult Invalid(string detail, HttpResponseMessage response, Exception? error = null)
    {
        return AttemptResult.Rejected(DaxFormatterFailureReason.InvalidResponse, detail, response, error);
    }

    private static Task<byte[]> ReadContentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
#if NET
        return response.Content.ReadAsByteArrayAsync(cancellationToken);
#else
        // The content is already in memory here, so a read that cannot be cancelled does not wait.
        cancellationToken.ThrowIfCancellationRequested();
        return response.Content.ReadAsByteArrayAsync();
#endif
    }
}
