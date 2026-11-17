namespace Dax.Formatter.Client.Http
{
    using Dax.Formatter.Models;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class DaxFormatterHttpClient : IDaxFormatterHttpClient, IDisposable
    {
        private const string MediaTypeNamesApplicationJson = "application/json";
        private const int DaxFormatterTimeoutSeconds = 60;

        private readonly JsonSerializerOptions _serializerOptions;
        private readonly HttpClient _httpClient;
        private readonly string? _application;
        private readonly string? _version;

        public DaxFormatterHttpClient(string? application, string? version)
        {
            _application = application;
            _version = version;

            var handler = new DaxFormatterHttpClientMessageHandler();

            _httpClient = new HttpClient(handler, disposeHandler: true);
            _httpClient.Timeout = TimeSpan.FromSeconds(DaxFormatterTimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNamesApplicationJson));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue(nameof(DecompressionMethods.GZip)));

            _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            _serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }

        public async Task<DaxFormatterResponse?> FormatAsync(DaxFormatterSingleRequest request, CancellationToken cancellationToken)
        {
            request.CallerApp = _application;
            request.CallerVersion = _version;

            var message = await FormatImplAsync(request, cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<DaxFormatterResponse>(message, _serializerOptions);
        }

        public async Task<IReadOnlyList<DaxFormatterResponse>> FormatAsync(DaxFormatterMultipleRequest request, CancellationToken cancellationToken)
        {
            request.CallerApp = _application;
            request.CallerVersion = _version;

            var message = await FormatImplAsync(request, cancellationToken).ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<IReadOnlyList<DaxFormatterResponse>>(message, _serializerOptions);
            return result ?? Array.Empty<DaxFormatterResponse>();
        }

        private async Task<string> FormatImplAsync<T>(T request, CancellationToken cancellationToken) where T : DaxFormatterRequest
        {
            cancellationToken.ThrowIfCancellationRequested();

            var json = JsonSerializer.Serialize(request, _serializerOptions);
            var uri = request.DaxTextFormatUri;

            using var content = new StringContent(json, Encoding.UTF8, MediaTypeNamesApplicationJson);
            using var response = await _httpClient.PostAsync(uri, content, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

#if NETSTANDARD
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#elif NET6_0_OR_GREATER
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
