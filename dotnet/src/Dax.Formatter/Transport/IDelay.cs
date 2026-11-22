namespace Dax.Formatter.Transport;

/// <summary>
/// The wait between two attempts of a request, which the tests replace so that they do not wait.
/// </summary>
internal interface IDelay
{
    Task WaitAsync(TimeSpan delay, CancellationToken cancellationToken);
}
