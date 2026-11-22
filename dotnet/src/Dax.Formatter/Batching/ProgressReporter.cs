namespace Dax.Formatter.Batching;

/// <summary>
/// Reports how many texts of a call are done. Requests end on different threads: each count joins a queue under
/// a lock, and one thread at a time passes the queue to the receiver, outside the lock. So the receiver gets the
/// counts one at a time and in increasing order, and a slow receiver never holds up the lock of the others.
/// </summary>
internal sealed class ProgressReporter
{
    private readonly object _gate = new();
    private readonly Queue<int> _pending = new();
    private readonly IProgress<DaxFormatProgressInfo>? _progress;
    private readonly int _total;
    private int _completed;
    private bool _draining;

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
            _pending.Enqueue(_completed);
            if (_draining)
            {
                return;
            }

            _draining = true;
        }

        Drain(_progress);
    }

    // Runs on the thread that found no other one draining, until the queue is empty.
    private void Drain(IProgress<DaxFormatProgressInfo> progress)
    {
        var emptied = false;
        try
        {
            while (true)
            {
                int completed;
                lock (_gate)
                {
                    if (_pending.Count == 0)
                    {
                        _draining = false;
                        emptied = true;
                        return;
                    }

                    completed = _pending.Dequeue();
                }

                progress.Report(new DaxFormatProgressInfo(completed, _total));
            }
        }
        finally
        {
            // A receiver that throws fails the call; the next count still finds no thread draining.
            if (!emptied)
            {
                lock (_gate)
                {
                    _draining = false;
                }
            }
        }
    }
}
