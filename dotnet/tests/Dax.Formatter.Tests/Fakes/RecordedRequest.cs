using System.Net.Http;
using System.Text.Json;

namespace Dax.Formatter.Tests.Fakes;

/// <summary>
/// A request that <see cref="FakeFormatterHandler"/> received, copied when it arrived.
/// </summary>
internal sealed class RecordedRequest
{
    private readonly Dictionary<string, string> _headers = new(StringComparer.OrdinalIgnoreCase);

    public RecordedRequest(int number, HttpRequestMessage request, byte[] body)
    {
        Number = number;
        Method = request.Method;
        Uri = request.RequestUri;
        Body = body;
        ContentType = request.Content?.Headers.ContentType?.ToString();

        foreach (var header in request.Headers)
        {
            _headers[header.Key] = string.Join(" ", header.Value);
        }

        using var document = JsonDocument.Parse(body);
        Json = document.RootElement.Clone();
    }

    /// <summary>Gets the position of the request among those the handler received, from zero.</summary>
    public int Number { get; }

    public HttpMethod Method { get; }

    public Uri? Uri { get; }

    public byte[] Body { get; }

    public string? ContentType { get; }

    public JsonElement Json { get; }

    public IReadOnlyList<string> Dax => [.. Json.GetProperty("dax").EnumerateArray().Select(text => text.GetString()!)];

    public string? GetHeader(string name)
    {
        return _headers.TryGetValue(name, out var value) ? value : null;
    }
}
