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
    public void Report_FromSeveralThreads_NeverReportsFewerTextsThanBefore()
    {
        var progress = new ListProgress();
        var reporter = new ProgressReporter(progress, total: 2000);

        Parallel.For(0, 2000, _ => reporter.Report(1));

        Assert.Equal(Enumerable.Range(1, 2000), progress.Reports.Select(report => report.Completed));
    }
}
