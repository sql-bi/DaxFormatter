namespace Dax.Formatter.Transport;

/// <summary>
/// The <see cref="HttpClient"/> of every client that was not given one. It is created at its first use and
/// lives as long as the process, so that all the clients share its connections.
/// </summary>
internal static class SharedHttpClient
{
    private static readonly Lazy<HttpClient> s_instance = new(Create);

    public static HttpClient Instance => s_instance.Value;

    private static HttpClient Create()
    {
#if NET
        var client = CreateWithPooledConnections();
#else
        var client = new HttpClient();
#endif

        // Each attempt has its own timer, which is the only one that must end it.
        client.Timeout = Timeout.InfiniteTimeSpan;
        return client;
    }

#if NET
    private static HttpClient CreateWithPooledConnections()
    {
        // In a browser there are no sockets: the default handler of the platform is the only one.
        if (!SocketsHttpHandler.IsSupported)
        {
            return new HttpClient();
        }

        SocketsHttpHandler? handler = null;
        try
        {
            handler = new SocketsHttpHandler();

            // Connections are replaced now and then, so that a change of the DNS record of the service is seen.
            handler.PooledConnectionLifetime = TimeSpan.FromMinutes(5);
            handler.ConnectTimeout = TimeSpan.FromSeconds(15);

            var client = new HttpClient(handler, disposeHandler: true);
            handler = null;
            return client;
        }
        finally
        {
            handler?.Dispose();
        }
    }
#endif
}
