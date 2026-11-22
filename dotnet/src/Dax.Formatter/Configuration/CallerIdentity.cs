using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Dax.Formatter.Configuration;

/// <summary>
/// The application that calls the service: the name and the version that go into the request body, and the
/// value of the <c>User-Agent</c> header.
/// </summary>
internal sealed class CallerIdentity
{
    private const string LibraryName = "Dax.Formatter";

    private CallerIdentity(string? applicationName, string? applicationVersion)
    {
        ApplicationName = applicationName;
        ApplicationVersion = applicationVersion;
        UserAgent = CreateUserAgent(applicationName, applicationVersion);
    }

    public string? ApplicationName { get; }

    public string? ApplicationVersion { get; }

    public IReadOnlyList<ProductInfoHeaderValue> UserAgent { get; }

    public static CallerIdentity Create(string? applicationName, string? applicationVersion)
    {
        return Create(applicationName, applicationVersion, Assembly.GetEntryAssembly());
    }

    public static CallerIdentity Create(string? applicationName, string? applicationVersion, Assembly? entryAssembly)
    {
        var name = NullIfBlank(applicationName) ?? NullIfBlank(entryAssembly?.GetName().Name);
        var fileVersion = entryAssembly?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        var version = NullIfBlank(applicationVersion) ?? NullIfBlank(fileVersion);

        return new CallerIdentity(name, version);
    }

    private static List<ProductInfoHeaderValue> CreateUserAgent(string? applicationName, string? applicationVersion)
    {
        var products = new List<ProductInfoHeaderValue>();

        var application = ToToken(applicationName);
        if (application is not null)
        {
            products.Add(new ProductInfoHeaderValue(application, ToToken(applicationVersion)));
        }

        products.Add(new ProductInfoHeaderValue(LibraryName, ToToken(GetLibraryVersion())));
        products.Add(new ProductInfoHeaderValue(ToComment(RuntimeInformation.FrameworkDescription)));

        return products;
    }

    private static string? GetLibraryVersion()
    {
        var assembly = typeof(CallerIdentity).Assembly;
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (version is null)
        {
            return assembly.GetName().Version?.ToString();
        }

        // The informational version ends with the commit id as build metadata, which says nothing to the service.
        return version.Split('+')[0];
    }

    private static string? NullIfBlank(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    // A product name or version is an HTTP token: "DAX Studio" becomes "DAX-Studio".
    private static string? ToToken(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var token = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (IsTokenCharacter(character))
            {
                token.Append(character);
            }
            else if (token.Length > 0 && token[token.Length - 1] != '-')
            {
                token.Append('-');
            }
        }

        var result = token.ToString().TrimEnd('-');
        return result.Length == 0 ? null : result;
    }

    private static bool IsTokenCharacter(char character)
    {
        if (character is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9'))
        {
            return true;
        }

        return character is '!' or '#' or '$' or '%' or '&' or '\'' or '*' or '+' or '-' or '.' or '^' or '_' or '`'
            or '|' or '~';
    }

    private static string ToComment(string value)
    {
        var comment = new StringBuilder(value.Length + 2);
        comment.Append('(');
        foreach (var character in value)
        {
            var allowed = character is >= ' ' and <= '~' and not ('(' or ')' or '\\');
            comment.Append(allowed ? character : ' ');
        }

        comment.Append(')');
        return comment.ToString();
    }
}
