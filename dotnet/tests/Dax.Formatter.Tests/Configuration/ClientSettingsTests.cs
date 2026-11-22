using Dax.Formatter.Configuration;
using Xunit;

namespace Dax.Formatter.Tests.Configuration;

public class ClientSettingsTests
{
    [Fact]
    public void From_DefaultOptions_UsesTheDefaults()
    {
        var settings = ClientSettings.From(new DaxFormatterClientOptions());

        Assert.Equal(new Uri("https://api.daxformatter.com/api/daxtextformatmulti"), settings.FormatUri);
        Assert.Equal(TimeSpan.FromSeconds(60), settings.RequestTimeout);
        Assert.Equal((2, 2), (settings.MaxRetries, settings.MaxConcurrentRequests));
    }

    [Theory]
    [InlineData("https://example.test", "https://example.test/api/daxtextformatmulti")]
    [InlineData("https://example.test/", "https://example.test/api/daxtextformatmulti")]
    [InlineData("https://example.test/base", "https://example.test/base/api/daxtextformatmulti")]
    [InlineData("http://localhost:7071/base/?x=1#y", "http://localhost:7071/base/api/daxtextformatmulti")]
    public void From_Endpoint_AppendsTheFormatPath(string endpoint, string expected)
    {
        var options = new DaxFormatterClientOptions { Endpoint = new Uri(endpoint) };

        var settings = ClientSettings.From(options);

        Assert.Equal(new Uri(expected), settings.FormatUri);
    }

    [Fact]
    public void From_Options_CopiesTheValues()
    {
        var options = new DaxFormatterClientOptions { MaxRetries = 5 };
        var settings = ClientSettings.From(options);

        options.MaxRetries = 7;

        Assert.Equal(5, settings.MaxRetries);
    }

    [Fact]
    public void From_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>("options", () => ClientSettings.From(null!));
    }

    [Theory]
    [InlineData(0, 2, 2)]
    [InlineData(-1, 2, 2)]
    [InlineData(60, -1, 2)]
    [InlineData(60, 2, 0)]
    public void From_ValueOutOfRange_Throws(int timeoutSeconds, int maxRetries, int maxConcurrentRequests)
    {
        var options = new DaxFormatterClientOptions
        {
            RequestTimeout = TimeSpan.FromSeconds(timeoutSeconds),
            MaxRetries = maxRetries,
            MaxConcurrentRequests = maxConcurrentRequests,
        };

        Assert.Throws<ArgumentException>("options", () => ClientSettings.From(options));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("api/format")]
    [InlineData("ftp://example.test/")]
    public void From_EndpointNotAbsoluteHttp_Throws(string? endpoint)
    {
        var options = new DaxFormatterClientOptions
        {
            Endpoint = endpoint is null ? null! : new Uri(endpoint, UriKind.RelativeOrAbsolute),
        };

        Assert.Throws<ArgumentException>("options", () => ClientSettings.From(options));
    }
}
