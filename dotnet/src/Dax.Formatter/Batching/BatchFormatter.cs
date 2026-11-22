using Dax.Formatter.Configuration;
using Dax.Formatter.Transport;
using Dax.Formatter.Wire;

namespace Dax.Formatter.Batching;

/// <summary>
/// Formats the texts of a call with as many requests as they need. One instance serves every call of a client.
/// </summary>
internal sealed class BatchFormatter
{
    private readonly RetryingSender _sender;
    private readonly RequestBodyWriter _bodyWriter;
    private readonly BatchLimits _limits;
    private readonly int _maxConcurrentRequests;

    private BatchFormatter(
        RetryingSender sender,
        RequestBodyWriter bodyWriter,
        BatchLimits limits,
        int maxConcurrentRequests)
    {
        _sender = sender;
        _bodyWriter = bodyWriter;
        _limits = limits;
        _maxConcurrentRequests = maxConcurrentRequests;
    }

    public static BatchFormatter Create(HttpClient httpClient, ClientSettings settings, IDelay delay, BatchLimits limits)
    {
        var caller = CallerIdentity.Create(settings.ApplicationName, settings.ApplicationVersion);
        var transport = new FormatterTransport(httpClient, settings, caller);
        var policy = new RetryPolicy(settings.MaxRetries, settings.RequestTimeout);
        var sender = new RetryingSender(transport, policy, delay);

        return new BatchFormatter(sender, new RequestBodyWriter(caller), limits, settings.MaxConcurrentRequests);
    }

    /// <summary>
    /// Formats the texts and returns their results in the same order.
    /// </summary>
    /// <exception cref="DaxFormatterException">A request failed.</exception>
    /// <exception cref="OperationCanceledException">The token was cancelled.</exception>
    public async Task<DaxFormatResult[]> FormatAsync(
        string[] texts,
        FormatSettings settings,
        IProgress<DaxFormatProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        var plan = BatchPlanner.Plan(texts, settings.Kind, _limits);
        var reporter = new ProgressReporter(progress, texts.Length);
        using var run = new BatchRun(plan, settings, _bodyWriter, _sender, reporter, cancellationToken);

        return await run.RunAsync(_maxConcurrentRequests).ConfigureAwait(false);
    }
}
