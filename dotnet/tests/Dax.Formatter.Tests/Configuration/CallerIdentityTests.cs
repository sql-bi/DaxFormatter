using System.Text.RegularExpressions;
using Dax.Formatter.Configuration;
using Xunit;

namespace Dax.Formatter.Tests.Configuration;

public class CallerIdentityTests
{
    [Fact]
    public void Create_NameAndVersion_BuildsTheUserAgent()
    {
        var identity = CallerIdentity.Create("Contoso", "1.2.3", entryAssembly: null);

        var userAgent = string.Join(" ", identity.UserAgent);

        Assert.Matches(new Regex(@"^Contoso/1\.2\.3 Dax\.Formatter/[0-9]+\.[0-9]+\.[0-9]+\S* \([^()]+\)$"), userAgent);
    }

    [Fact]
    public void Create_NameWithSpace_KeepsItInTheBodyAndCleansItInTheUserAgent()
    {
        var identity = CallerIdentity.Create("DAX Studio", "3.0.11", entryAssembly: null);

        Assert.Equal("DAX Studio", identity.ApplicationName);
        Assert.StartsWith("DAX-Studio/3.0.11 ", string.Join(" ", identity.UserAgent), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("a/b  c", "1.0 (beta)", "a-b-c/1.0-beta ")]
    [InlineData("Größe", "1", "Gr-e/1 ")]
    [InlineData("App", "(1)", "App/1 ")]
    [InlineData("App", "()", "App ")]
    [InlineData("App", null, "App ")]
    public void Create_CharactersOutsideHttpTokens_ReplacesThem(string name, string? version, string expectedStart)
    {
        var identity = CallerIdentity.Create(name, version, entryAssembly: null);

        Assert.StartsWith(expectedStart + "Dax.Formatter/", string.Join(" ", identity.UserAgent), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("()")]
    public void Create_NoUsableNameAndNoEntryAssembly_LeavesTheApplicationOutOfTheUserAgent(string? name)
    {
        var identity = CallerIdentity.Create(name, "1.0", entryAssembly: null);

        Assert.StartsWith("Dax.Formatter/", string.Join(" ", identity.UserAgent), StringComparison.Ordinal);
    }

    [Fact]
    public void Create_NoName_UsesTheEntryAssembly()
    {
        var assembly = typeof(CallerIdentityTests).Assembly;

        var identity = CallerIdentity.Create(applicationName: null, applicationVersion: null, assembly);

        Assert.Equal("Dax.Formatter.Tests", identity.ApplicationName);
        Assert.False(string.IsNullOrEmpty(identity.ApplicationVersion));
    }

    [Fact]
    public void Create_NoNameAndNoEntryAssembly_HasNoApplication()
    {
        var identity = CallerIdentity.Create(applicationName: null, applicationVersion: null, entryAssembly: null);

        Assert.Null(identity.ApplicationName);
        Assert.Null(identity.ApplicationVersion);
    }
}
