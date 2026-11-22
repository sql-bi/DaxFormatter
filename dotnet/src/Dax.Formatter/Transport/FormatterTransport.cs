using System.Globalization;
using System.Net.Http.Headers;
using Dax.Formatter.Configuration;

namespace Dax.Formatter.Transport;

/// <summary>
/// Makes one attempt to send a request: headers, timeout, and the reading of the response.
/// </summary>
internal sealed class FormatterTransport
{
    private const string RequestIdHeader = "DaxFormatter-Request-Id";
    private const string OperationIdHeader = "DaxFormatter-Operation-Id";

    private const string JsonMediaType = "application/json";

    private static readonly MediaTypeWithQualityHeaderValue s_json = new(JsonMediaType);

    private readonly HttpClient _httpClient;
    private readonly Uri _formatUri;
    private readonly TimeSpan _requestTimeout;
    private readonly CallerIdentity _caller;

    public FormatterTransport(HttpClient httpClient, ClientSettings settings, CallerIdentity caller)
    {
        _httpClient = httpClient;
        _formatUri = settings.FormatUri;
        _requestTimeout = settings.RequestTimeout;
        _caller = caller;
    }

    /// <summary>
    /// Sends the request once. A failure of the service or of the network comes back as a result; the only
    /// exception is <see cref="OperationCanceledException"/>, when <paramref name="cancellationToken"/> is
    /// cancelled.
    /// </summary>
    public async Task<AttemptResult> SendAsync(FormatRequest request, CancellationToken cancellationToken)
    {
        using var message = CreateMessage(request);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_requestTimeout);

        try
        {
            using var response = await _httpClient
                .SendAsync(message, HttpCompletionOption.ResponseContentRead, timeout.Token)
                .ConfigureAwait(false);

            return await ResponseReader.ReadAsync(response, request.TextCount, timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException exception)
        {
            // Without a cancellation from the caller, what ended the wait is a timer: the one of this attempt
            // or the Timeout of an HttpClient that the caller supplied.
            cancellationToken.ThrowIfCancellationRequested();

            var seconds = _requestTimeout.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture);
            var detail = $"the service did not answer within {seconds} seconds";
            return AttemptResult.NoResponse(DaxFormatterFailureReason.Timeout, detail, exception);
        }
        catch (HttpRequestException exception)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var detail = $"the service could not be reached ({exception.Message})";
            return AttemptResult.NoResponse(DaxFormatterFailureReason.NetworkError, detail, exception);
        }
    }

    private HttpRequestMessage CreateMessage(FormatRequest request)
    {
        // The URI is absolute, so the BaseAddress of an HttpClient that the caller supplied plays no part.
        var message = new HttpRequestMessage(HttpMethod.Post, _formatUri);
        message.Headers.Accept.Add(s_json);
        message.Headers.ExpectContinue = false;
        message.Headers.Add(RequestIdHeader, request.RequestId.ToString());
        message.Headers.Add(OperationIdHeader, request.OperationId.ToString());
        foreach (var product in _caller.UserAgent)
        {
            message.Headers.UserAgent.Add(product);
        }

        message.Content = new ByteArrayContent(request.Body);
        message.Content.Headers.ContentType = new MediaTypeHeaderValue(JsonMediaType) { CharSet = "utf-8" };

        return message;
    }
}
