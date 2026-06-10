namespace Dax.Formatter.Client.Http
{
    using Dax.Formatter.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
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
        private readonly SemaphoreSlim _formatSemaphore;
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
            
            _formatSemaphore = new SemaphoreSlim(1);

            _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            _serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }

        public async Task<DaxFormatterResponse?> FormatAsync(DaxFormatterSingleRequest request, CancellationToken cancellationToken)
        {
#if NETSTANDARD
            await _formatSemaphore.WaitAsync().ConfigureAwait(false);
#elif NET6_0_OR_GREATER
            await _formatSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
#endif
            try
            {
                request.CallerApp = _application;
                request.CallerVersion = _version;

                var message = await FormatImplAsync(request, cancellationToken).ConfigureAwait(false);
                var result = JsonSerializer.Deserialize<DaxFormatterResponse>(message, _serializerOptions);

                return result;
            }
            finally
            {
                _formatSemaphore.Release();
            }
        }

        public async Task<IReadOnlyList<DaxFormatterResponse>> FormatAsync(DaxFormatterMultipleRequest request, CancellationToken cancellationToken)
        {
#if NETSTANDARD
            await _formatSemaphore.WaitAsync().ConfigureAwait(false);
#elif NET6_0_OR_GREATER
            await _formatSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
#endif
            try
            {
                request.CallerApp = _application;
                request.CallerVersion = _version;

                var message = await FormatImplAsync(request, cancellationToken).ConfigureAwait(false);
                var result = JsonSerializer.Deserialize<IReadOnlyList<DaxFormatterResponse>>(message, _serializerOptions);

                return result ?? Array.Empty<DaxFormatterResponse>();
            }
            finally
            {
                _formatSemaphore.Release();
            }
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
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#elif NET6_0_OR_GREATER
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#endif
            using var reader = new StreamReader(stream);
            var message = reader.ReadToEnd();

            return message;
        }

        public void Dispose()
        {
            _formatSemaphore.Dispose();
            _httpClient.Dispose();
        }
    }
}
