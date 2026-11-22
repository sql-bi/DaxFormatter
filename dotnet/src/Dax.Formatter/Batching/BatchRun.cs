using Dax.Formatter.Configuration;
using Dax.Formatter.Results;
using Dax.Formatter.Transport;
using Dax.Formatter.Wire;

namespace Dax.Formatter.Batching;

/// <summary>
/// One call in progress: a few workers take the requests of the plan in turn and write each result at the index
/// of its text. The first request that fails stops the others, and the call fails with it.
/// </summary>
internal sealed class BatchRun : IDisposable
{
    private readonly BatchPlan _plan;
    private readonly FormatSettings _settings;
    private readonly RequestBodyWriter _bodyWriter;
    private readonly RetryingSender _sender;
    private readonly ProgressReporter _progress;
    private readonly DaxFormatResult[] _results;
    private readonly Guid _operationId = Guid.NewGuid();
    private readonly CancellationToken _callerToken;

    // The workers watch the token of this source: the caller cancels it, and so does the first request that fails.
    private readonly CancellationTokenSource _stop;

    private int _nextChunk;
    private DaxFormatterException? _failure;
    private volatile bool _stopped;

    public BatchRun(
        BatchPlan plan,
        FormatSettings settings,
        RequestBodyWriter bodyWriter,
        RetryingSender sender,
        ProgressReporter progress,
        CancellationToken cancellationToken)
    {
        _plan = plan;
        _settings = settings;
        _bodyWriter = bodyWriter;
        _sender = sender;
        _progress = progress;
        _results = new DaxFormatResult[plan.TextCount];
        _callerToken = cancellationToken;
        _stop = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    }

    public async Task<DaxFormatResult[]> RunAsync(int maxConcurrentRequests)
    {
        _callerToken.ThrowIfCancellationRequested();
        CompleteBlankTexts();

        var workers = new Task[Math.Min(maxConcurrentRequests, _plan.Chunks.Count)];
        for (var i = 0; i < workers.Length; i++)
        {
            workers[i] = WorkAsync();
        }

        await Task.WhenAll(workers).ConfigureAwait(false);

        if (!_stopped)
        {
            return _results;
        }

        // A cancellation by the caller wins over a failure that happened at the same time.
        _callerToken.ThrowIfCancellationRequested();
        throw _failure ?? new DaxFormatterException("The call stopped before every request was sent.");
    }

    public void Dispose()
    {
        _stop.Dispose();
    }

    // A blank text is not sent: the service would format it to an empty text.
    private void CompleteBlankTexts()
    {
        foreach (var index in _plan.BlankIndexes)
        {
            _results[index] = DaxFormatResult.Success(_plan.GetText(index), string.Empty);
        }

        _progress.Report(_plan.BlankIndexes.Count);
    }

    private async Task WorkAsync()
    {
        var finished = false;
        try
        {
            while (true)
            {
                var index = Interlocked.Increment(ref _nextChunk) - 1;
                if (index >= _plan.Chunks.Count)
                {
                    finished = true;
                    return;
                }

                _stop.Token.ThrowIfCancellationRequested();
                await FormatChunkAsync(_plan.Chunks[index], _stop.Token).ConfigureAwait(false);
            }
        }
        catch (DaxFormatterException exception)
        {
            Interlocked.CompareExchange(ref _failure, exception, null);
        }
        catch (OperationCanceledException) when (_stop.IsCancellationRequested)
        {
            // The caller or a failed request stopped the call: RunAsync tells which. A cancellation that comes
            // from elsewhere, such as the progress receiver of the caller, is not swallowed.
        }
        finally
        {
            if (!finished)
            {
                _stopped = true;
                await StopOthersAsync().ConfigureAwait(false);
            }
        }
    }

    private async Task FormatChunkAsync(Chunk chunk, CancellationToken cancellationToken)
    {
        var request = CreateRequest(chunk);
        var response = await _sender.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var items = response.Items!;

        for (var position = 0; position < chunk.Count; position++)
        {
            var index = chunk[position];
            var header = _plan.GetHeader(index);
            if (!ResultMapper.TryMap(_plan.GetText(index), header, items[position], out var result))
            {
                var detail = FormattableString.Invariant(
                    $"the result of the text at index {index} does not start with the header that the client added");
                throw request.CreateException(
                    DaxFormatterFailureReason.InvalidResponse,
                    detail,
                    response.StatusCode,
                    response.ServiceRequestId,
                    innerException: null);
            }

            _results[index] = result;
        }

        _progress.Report(chunk.Count);
    }

    private FormatRequest CreateRequest(Chunk chunk)
    {
        var body = _bodyWriter.Write(_plan.GetTextsToSend(chunk), _settings);
        return new FormatRequest(body, chunk.Count, chunk.FirstIndex, chunk.LastIndex, _operationId);
    }

    private Task StopOthersAsync()
    {
#if NET
        return _stop.CancelAsync();
#else
        _stop.Cancel();
        return Task.CompletedTask;
#endif
    }
}
