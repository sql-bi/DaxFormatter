using Xunit;

namespace Dax.Formatter.Tests;

public class DaxFormatProgressInfoTests
{
    [Fact]
    public void Constructor_Counts_KeepsThem()
    {
        var progress = new DaxFormatProgressInfo(250, 1000);

        Assert.Equal((250, 1000), (progress.Completed, progress.Total));
    }

    [Fact]
    public void Constructor_NegativeCount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>("completed", () => new DaxFormatProgressInfo(-1, 10));
        Assert.Throws<ArgumentOutOfRangeException>("total", () => new DaxFormatProgressInfo(0, -1));
    }
}
