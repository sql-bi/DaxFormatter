namespace Dax.Formatter.Client.Http
{
    using System.Net;
    using System.Net.Http;

    internal class DaxFormatterHttpClientMessageHandler : HttpClientHandler
    {
        public DaxFormatterHttpClientMessageHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip;
        }
    }
}
