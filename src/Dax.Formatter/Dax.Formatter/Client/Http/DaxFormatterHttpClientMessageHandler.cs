namespace Dax.Formatter.Client.Http
{
    using System.Net;
    using System.Net.Http;
    using System.Security.Authentication;

    internal class DaxFormatterHttpClientMessageHandler : HttpClientHandler
    {
        public DaxFormatterHttpClientMessageHandler()
        {
            AllowAutoRedirect = false;
            SslProtocols = SslProtocols.Tls12;
            AutomaticDecompression = DecompressionMethods.GZip;
        }
    }
}
