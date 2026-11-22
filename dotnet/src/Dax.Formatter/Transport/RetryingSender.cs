namespace Dax.Formatter.Transport;

/// <summary>
/// Sends a request until an attempt succeeds or the retry policy gives up, then throws the failure of the last
/// attempt.
/// </summary>
internal sealed class RetryingSender
{
    private readonly FormatterTransport _transport;
    private readonly RetryPolicy _policy;
    private readonly IDelay _delay;

    public RetryingSender(FormatterTransport transport, RetryPolicy policy, IDelay delay)
    {
        _transport = transport;
        _policy = policy;
        _delay = delay;
    }

    /// <exception cref="DaxFormatterException">The request failed and is not tried again.</exception>
    /// <exception cref="OperationCanceledException">The token was cancelled.</exception>
    public async Task<AttemptResult> SendAsync(FormatRequest request, CancellationToken cancellationToken)
    {
        var attempt = 1;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await _transport.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (result.Succeeded)
            {
                return result;
            }

            if (!_policy.TryGetDelay(result, attempt, request.RequestId, out var delay))
            {
                throw CreateException(request, result, result.Detail, attempt);
            }

            await _delay.WaitAsync(delay, cancellationToken).ConfigureAwait(false);
            attempt++;
        }
    }

    private static DaxFormatterException CreateException(
        FormatRequest request,
        AttemptResult failure,
        string detail,
        int attempts)
    {
        if (attempts > 1)
        {
            detail = FormattableString.Invariant($"{detail}, after {attempts} attempts");
        }

        return request.CreateException(
            failure.Reason,
            detail,
            failure.StatusCode,
            failure.ServiceRequestId,
            failure.Error);
    }
}
