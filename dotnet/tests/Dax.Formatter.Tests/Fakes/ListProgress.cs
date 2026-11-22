namespace Dax.Formatter.Tests.Fakes;

/// <summary>
/// Collects progress reports in the order they are made, on the thread that makes them.
/// </summary>
internal sealed class ListProgress : IProgress<DaxFormatProgressInfo>
{
    private readonly List<DaxFormatProgressInfo> _reports = [];

    public IReadOnlyList<(int Completed, int Total)> Reports
    {
        get
        {
            lock (_reports)
            {
                return [.. _reports.Select(report => (report.Completed, report.Total))];
            }
        }
    }

    public void Report(DaxFormatProgressInfo value)
    {
        lock (_reports)
        {
            _reports.Add(value);
        }
    }
}
