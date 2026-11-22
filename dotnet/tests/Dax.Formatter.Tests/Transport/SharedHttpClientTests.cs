using Dax.Formatter.Transport;
using Xunit;

namespace Dax.Formatter.Tests.Transport;

public class SharedHttpClientTests
{
    [Fact]
    public void Instance_AnyUse_ReturnsTheSameClientWithoutItsOwnTimeout()
    {
        var first = SharedHttpClient.Instance;
        var second = SharedHttpClient.Instance;

        Assert.Same(first, second);
        Assert.Equal(Timeout.InfiniteTimeSpan, first.Timeout);
    }
}
