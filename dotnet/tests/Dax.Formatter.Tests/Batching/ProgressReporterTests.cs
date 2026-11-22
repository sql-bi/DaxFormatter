using Dax.Formatter.Batching;
using Dax.Formatter.Configuration;
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
    public async Task FormatBatchAsync_BlankTexts_CountsThemBeforeTheFirstRequest()
    {
        var progress = new ListProgress();
        using var harness = new FormatterHarness(
            Respond.Echo,
            options => options.MaxConcurrentRequests = 1,
            new BatchLimits(maxTexts: 1, maxCharacters: 1000));

        await harness.Client.FormatBatchAsync(["", "a", "b", " "], progress: progress).ConfigureAwait(true);

        Assert.Equal([(2, 4), (3, 4), (4, 4)], progress.Reports);
    }

    [Fact]
    public async Task FormatBatchAsync_RequestsInParallel_NeverReportsFewerTextsThanBefore()
    {
        var progress = new ListProgress();
        var texts = Enumerable.Range(0, 40).Select(number => "EVALUATE T" + number);
        using var harness = new FormatterHarness(
            Respond.Echo,
            options => options.MaxConcurrentRequests = 4,
            new BatchLimits(maxTexts: 3, maxCharacters: 1000));

        await harness.Client.FormatBatchAsync(texts, progress: progress).ConfigureAwait(true);

        var completed = progress.Reports.Select(report => report.Completed).ToList();
        Assert.Equal(completed.OrderBy(count => count), completed);
        Assert.Equal((40, 40), progress.Reports[progress.Reports.Count - 1]);
    }
}
