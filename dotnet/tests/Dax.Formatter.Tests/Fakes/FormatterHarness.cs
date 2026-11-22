using System.Net.Http;
using Dax.Formatter.Configuration;

namespace Dax.Formatter.Tests.Fakes;

/// <summary>
/// A client wired to a <see cref="FakeFormatterHandler"/> and a <see cref="RecordingDelay"/>, with what a test
/// needs to look at afterwards.
/// </summary>
internal sealed class FormatterHarness : IDisposable
{
    public const string ApplicationName = "Test App";
    public const string ApplicationVersion = "1.2.3";

    public FormatterHarness(
        Responder responder,
        Action<DaxFormatterClientOptions>? configure = null,
        BatchLimits? limits = null)
    {
        var options = new DaxFormatterClientOptions
        {
            ApplicationName = ApplicationName,
            ApplicationVersion = ApplicationVersion,
        };
        configure?.Invoke(options);

        Handler = new FakeFormatterHandler(responder);
        HttpClient = new HttpClient(Handler);
        Delay = new RecordingDelay();
        Client = new DaxFormatterClient(HttpClient, options, Delay, limits ?? BatchLimits.Default);
    }

    public FakeFormatterHandler Handler { get; }

    public HttpClient HttpClient { get; }

    public RecordingDelay Delay { get; }

    public DaxFormatterClient Client { get; }

    public void Dispose()
    {
        HttpClient.Dispose();
        Handler.Dispose();
    }
}
