namespace Dax.Formatter.Tests.Readme;

/// <summary>
/// The client that the samples of the readme create. It has the members of <see cref="DaxFormatterClient"/> and
/// the constructor that the samples call, and sends its requests to <see cref="ReadmeService"/>: the samples run
/// as they are written, without the live service.
/// </summary>
internal sealed class ReadmeClient : DaxFormatterClient
{
    public ReadmeClient()
        : base(ReadmeService.HttpClient)
    {
    }
}
