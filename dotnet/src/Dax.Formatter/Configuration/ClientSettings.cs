namespace Dax.Formatter.Configuration;

/// <summary>
/// The validated, immutable copy of a <see cref="DaxFormatterClientOptions"/>.
/// </summary>
internal sealed class ClientSettings
{
    private const string FormatPath = "api/daxtextformatmulti";

    private ClientSettings(DaxFormatterClientOptions options, Uri formatUri)
    {
        ApplicationName = options.ApplicationName;
        ApplicationVersion = options.ApplicationVersion;
        FormatUri = formatUri;
        RequestTimeout = options.RequestTimeout;
        MaxRetries = options.MaxRetries;
        MaxConcurrentRequests = options.MaxConcurrentRequests;
    }

    public string? ApplicationName { get; }

    public string? ApplicationVersion { get; }

    public Uri FormatUri { get; }

    public TimeSpan RequestTimeout { get; }

    public int MaxRetries { get; }

    public int MaxConcurrentRequests { get; }

    public static ClientSettings From(DaxFormatterClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // A timer accepts at most this many milliseconds on every target.
        var longestTimeout = TimeSpan.FromMilliseconds(int.MaxValue);
        if (options.RequestTimeout <= TimeSpan.Zero || options.RequestTimeout > longestTimeout)
        {
            throw new ArgumentException("RequestTimeout must be positive and at most 24 days.", nameof(options));
        }

        if (options.MaxRetries < 0)
        {
            throw new ArgumentException("MaxRetries must not be negative.", nameof(options));
        }

        if (options.MaxConcurrentRequests < 1)
        {
            throw new ArgumentException("MaxConcurrentRequests must be at least 1.", nameof(options));
        }

        return new ClientSettings(options, CreateFormatUri(options));
    }

    private static Uri CreateFormatUri(DaxFormatterClientOptions options)
    {
        var endpoint = options.Endpoint;
        if (endpoint is null || !endpoint.IsAbsoluteUri)
        {
            throw new ArgumentException("Endpoint must be an absolute URI.", nameof(options));
        }

        if (endpoint.Scheme != Uri.UriSchemeHttps && endpoint.Scheme != Uri.UriSchemeHttp)
        {
            throw new ArgumentException("Endpoint must be an http or https URI.", nameof(options));
        }

        // A relative URI replaces the last segment of a base that has no final slash.
        var basePath = endpoint.GetLeftPart(UriPartial.Path);
        if (basePath[basePath.Length - 1] != '/')
        {
            basePath += "/";
        }

        return new Uri(new Uri(basePath), FormatPath);
    }
}
