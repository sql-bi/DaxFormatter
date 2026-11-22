namespace Dax.Formatter.Transport;

/// <summary>
/// The wait that the client uses outside tests.
/// </summary>
internal sealed class TaskDelay : IDelay
{
    public static readonly TaskDelay Instance = new();

    private TaskDelay()
    {
    }

    public Task WaitAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        return Task.Delay(delay, cancellationToken);
    }
}
