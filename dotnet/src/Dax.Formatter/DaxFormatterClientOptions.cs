namespace Dax.Formatter;

/// <summary>
/// The options of a <see cref="DaxFormatterClient"/>. The client copies them when it is created: a later change
/// to this object has no effect on it.
/// </summary>
public sealed class DaxFormatterClientOptions
{
    /// <summary>
    /// Gets or sets the name of the calling application, which the service uses for usage statistics. The default
    /// is the name of the entry assembly.
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Gets or sets the version of the calling application. The default is the file version of the entry assembly.
    /// </summary>
    public string? ApplicationVersion { get; set; }

    /// <summary>
    /// Gets or sets the base address of the service. The default is <c>https://api.daxformatter.com/</c>.
    /// </summary>
    public Uri Endpoint { get; set; } = new Uri("https://api.daxformatter.com/");

    /// <summary>
    /// Gets or sets how long the client waits for the answer to one request, before it retries or gives up. The
    /// default is 60 seconds. A call that needs several requests or retries can last longer.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Gets or sets how many times the client sends a request again after a failure that can be temporary. The
    /// default is 2; 0 turns retries off.
    /// </summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>
    /// Gets or sets how many requests one call can have in progress at the same time, when it has more texts than
    /// one request carries. The default is 2.
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 2;
}
