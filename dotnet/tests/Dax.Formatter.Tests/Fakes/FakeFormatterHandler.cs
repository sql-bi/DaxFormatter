using System.Net.Http;

namespace Dax.Formatter.Tests.Fakes;

/// <summary>
/// Answers a request in place of the service.
/// </summary>
internal delegate Task<HttpResponseMessage> Responder(RecordedRequest request, CancellationToken cancellationToken);

/// <summary>
/// Stands in for the service: records each request it receives and answers with a <see cref="Responder"/>.
/// </summary>
internal sealed class FakeFormatterHandler : HttpMessageHandler
{
    private readonly object _gate = new();
    private readonly List<RecordedRequest> _requests = [];
    private readonly Responder _responder;
    private int _inFlight;
    private int _maxInFlight;

    public FakeFormatterHandler(Responder responder)
    {
        _responder = responder;
    }

    public IReadOnlyList<RecordedRequest> Requests
    {
        get
        {
            lock (_gate)
            {
                return [.. _requests];
            }
        }
    }

    /// <summary>Gets the largest number of requests that were in progress at the same time.</summary>
    public int MaxInFlight
    {
        get
        {
            lock (_gate)
            {
                return _maxInFlight;
            }
        }
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // The body is read before anything else: the client disposes the request when the call ends.
#if NET
        var body = await request.Content!.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
        var body = await request.Content!.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif

        RecordedRequest recorded;
        lock (_gate)
        {
            recorded = new RecordedRequest(_requests.Count, request, body);
            _requests.Add(recorded);
            _inFlight++;
            _maxInFlight = Math.Max(_maxInFlight, _inFlight);
        }

        try
        {
            return await _responder(recorded, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            lock (_gate)
            {
                _inFlight--;
            }
        }
    }
}
