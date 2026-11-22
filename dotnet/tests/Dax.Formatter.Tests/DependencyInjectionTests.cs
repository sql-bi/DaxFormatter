using Dax.Formatter.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dax.Formatter.Tests;

/// <summary>
/// Keeps <see cref="DaxFormatterClient"/> a typed client of <c>IHttpClientFactory</c>: the factory can create it
/// only while one constructor alone takes an <see cref="HttpClient"/>.
/// </summary>
public class DependencyInjectionTests
{
    [Fact]
    public async Task AddHttpClient_TypedClient_SendsTheRequestsWithTheConfiguredHandler()
    {
        using var handler = new FakeFormatterHandler(Respond.Echo);
        var services = new ServiceCollection();
        services.AddHttpClient<DaxFormatterClient>().ConfigurePrimaryHttpMessageHandler(() => handler);
        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<DaxFormatterClient>();

        await client.FormatAsync("EVALUATE Sales");

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task AddHttpClient_OptionsInTheContainer_CreatesTheClientWithThem()
    {
        using var handler = new FakeFormatterHandler(Respond.Echo);
        var services = new ServiceCollection();
        services.AddSingleton(new DaxFormatterClientOptions { ApplicationName = "Contoso Model Tools" });
        services.AddHttpClient<DaxFormatterClient>().ConfigurePrimaryHttpMessageHandler(() => handler);
        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<DaxFormatterClient>();

        await client.FormatAsync("EVALUATE Sales");

        Assert.Equal("Contoso Model Tools", handler.Requests[0].Json.GetProperty("callerApp").GetString());
    }
}
