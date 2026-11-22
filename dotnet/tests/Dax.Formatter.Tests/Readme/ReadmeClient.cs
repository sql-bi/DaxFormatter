using System.Net.Http;

namespace Dax.Formatter.Tests.Readme;

/// <summary>
/// The client that the samples of the readme create. It has the public constructors and members of
/// <see cref="DaxFormatterClient"/>, and sends its requests to <see cref="ReadmeService"/> unless it is given
/// an <see cref="HttpClient"/>: the samples run as they are written, without the live service.
/// </summary>
internal sealed class ReadmeClient : DaxFormatterClient
{
    public ReadmeClient()
        : base(ReadmeService.HttpClient)
    {
    }

    public ReadmeClient(DaxFormatterClientOptions options)
        : base(ReadmeService.HttpClient, options)
    {
    }

    public ReadmeClient(HttpClient httpClient)
        : base(httpClient)
    {
    }
}
