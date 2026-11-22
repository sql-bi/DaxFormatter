using Dax.Formatter.Transport;

namespace Dax.Formatter.Tests.Fakes;

/// <summary>
/// Records the waits between attempts and does not wait, so that no test does.
/// </summary>
internal sealed class RecordingDelay : IDelay
{
    private readonly List<TimeSpan> _delays = [];

    public IReadOnlyList<TimeSpan> Delays
    {
        get
        {
            lock (_delays)
            {
                return [.. _delays];
            }
        }
    }

    /// <summary>Gets or sets what happens when a wait starts, such as a cancellation by the caller.</summary>
    public Action? OnWait { get; set; }

    public Task WaitAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        lock (_delays)
        {
            _delays.Add(delay);
        }

        OnWait?.Invoke();

        return cancellationToken.IsCancellationRequested
            ? Task.FromCanceled(cancellationToken)
            : Task.CompletedTask;
    }
}
