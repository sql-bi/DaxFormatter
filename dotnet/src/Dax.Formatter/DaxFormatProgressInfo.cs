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
    /// Gets the number of texts whose result is ready. It never decreases during a call.
    /// </summary>
    public int Completed { get; }

    /// <summary>
    /// Gets the number of texts in the call.
    /// </summary>
    public int Total { get; }
}
