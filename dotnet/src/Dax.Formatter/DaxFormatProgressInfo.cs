namespace Dax.Formatter;

/// <summary>
/// The progress of a call that formats several texts.
/// </summary>
public sealed class DaxFormatProgressInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DaxFormatProgressInfo"/> class.
    /// </summary>
    /// <param name="completed">The number of texts whose result is ready.</param>
    /// <param name="total">The number of texts in the call.</param>
    public DaxFormatProgressInfo(int completed, int total)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(completed);
        ArgumentOutOfRangeException.ThrowIfNegative(total);

        Completed = completed;
        Total = total;
    }

    /// <summary>
    /// Gets the number of texts whose result is ready.
    /// </summary>
    /// <remarks>
    /// The client reports the values of a call one at a time and in increasing order. A
    /// <see cref="Progress{T}"/> created without a <see cref="SynchronizationContext"/>, as in a console
    /// application, runs its handler on the thread pool, where two reports can run in the opposite order: keep the
    /// largest value received.
    /// </remarks>
    public int Completed { get; }

    /// <summary>
    /// Gets the number of texts in the call.
    /// </summary>
    public int Total { get; }
}
