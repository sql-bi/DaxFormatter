namespace Dax.Formatter.Batching;

/// <summary>
/// Reports how many texts of a call are done. Requests end on different threads, so the count and the report
/// happen together under a lock: the reported count never goes backwards.
/// </summary>
internal sealed class ProgressReporter
{
    private readonly object _gate = new();
    private readonly IProgress<DaxFormatProgressInfo>? _progress;
    private readonly int _total;
    private int _completed;

    public ProgressReporter(IProgress<DaxFormatProgressInfo>? progress, int total)
    {
        _progress = progress;
        _total = total;
    }

    public void Report(int newlyCompleted)
    {
        if (_progress is null || newlyCompleted == 0)
        {
            return;
        }

        lock (_gate)
        {
            _completed += newlyCompleted;
            _progress.Report(new DaxFormatProgressInfo(_completed, _total));
        }
    }
}
