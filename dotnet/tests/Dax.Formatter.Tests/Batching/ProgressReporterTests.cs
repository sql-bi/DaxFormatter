using Dax.Formatter.Batching;
using Dax.Formatter.Tests.Fakes;
using Xunit;

namespace Dax.Formatter.Tests.Batching;

public class ProgressReporterTests
{
    [Fact]
    public void Report_SeveralTimes_AddsUpTheTextsDone()
    {
        var progress = new ListProgress();
        var reporter = new ProgressReporter(progress, total: 10);

        reporter.Report(2);
        reporter.Report(3);

        Assert.Equal([(2, 10), (5, 10)], progress.Reports);
    }

    [Fact]
    public void Report_NoTextsDone_ReportsNothing()
    {
        var progress = new ListProgress();
        var reporter = new ProgressReporter(progress, total: 10);

        reporter.Report(0);

        Assert.Empty(progress.Reports);
    }

    [Fact]
    public void Report_NoReceiver_DoesNothing()
    {
        var reporter = new ProgressReporter(progress: null, total: 10);

        var exception = Record.Exception(() => reporter.Report(2));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Report_WhileTheReceiverIsBusy_ReturnsAndLeavesTheCountToIt()
    {
        using var progress = new BlockingProgress();
        var reporter = new ProgressReporter(progress, total: 10);

        var first = Task.Run(() => reporter.Report(1));
        progress.Receiving.Wait();
        var second = Task.Run(() => reporter.Report(2));
        var finished = await Task.WhenAny(second, Task.Delay(TimeSpan.FromSeconds(10))).ConfigureAwait(true);
        progress.Release.Set();
        await first.ConfigureAwait(true);

        Assert.Same(second, finished);
        Assert.Equal([1, 3], progress.Values);
    }

    [Fact]
    public void Report_FromSeveralThreads_NeverReportsFewerTextsThanBefore()
    {
        var progress = new ListProgress();
        var reporter = new ProgressReporter(progress, total: 2000);

        Parallel.For(0, 2000, _ => reporter.Report(1));

        Assert.Equal(Enumerable.Range(1, 2000), progress.Reports.Select(report => report.Completed));
    }

    // A receiver that holds the first report until the test releases it.
    private sealed class BlockingProgress : IProgress<DaxFormatProgressInfo>, IDisposable
    {
        private readonly List<int> _values = [];

        public ManualResetEventSlim Receiving { get; } = new();

        public ManualResetEventSlim Release { get; } = new();

        public IReadOnlyList<int> Values => _values;

        public void Report(DaxFormatProgressInfo value)
        {
            _values.Add(value.Completed);
            if (_values.Count == 1)
            {
                Receiving.Set();
                Release.Wait();
            }
        }

        public void Dispose()
        {
            Receiving.Dispose();
            Release.Dispose();
        }
    }
}
