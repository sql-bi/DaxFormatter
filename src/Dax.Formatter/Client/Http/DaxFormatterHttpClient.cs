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

    internal class DaxFormatterHttpClient : IDaxFormatterHttpClient, IDisposable
    {
        private const string MediaTypeNamesApplicationJson = "application/json";
        private const int DaxFormatterTimeoutSeconds = 60;

        private readonly HashSet<HttpStatusCode> _locationChangedStatusCodes;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly SemaphoreSlim _initializeServiceUriSemaphore;
        private readonly SemaphoreSlim _formatSemaphore;
        private readonly HttpClient _httpClient;

        private Uri? _daxTextFormatSingleServiceUri;
        private Uri? _daxTextFormatMultiServiceUri;
        private bool _disposed;

        public DaxFormatterHttpClient()
        {
            var handler = new DaxFormatterHttpClientMessageHandler();

            _httpClient = new HttpClient(handler, disposeHandler: true);
            _httpClient.Timeout = TimeSpan.FromSeconds(DaxFormatterTimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNamesApplicationJson));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue(nameof(DecompressionMethods.GZip)));
            
            _initializeServiceUriSemaphore = new SemaphoreSlim(1);
            _formatSemaphore = new SemaphoreSlim(1);

            _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            _serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            
            _locationChangedStatusCodes = new HashSet<HttpStatusCode>
            {
                HttpStatusCode.Moved,
                HttpStatusCode.MovedPermanently,
                HttpStatusCode.Found,
                HttpStatusCode.Redirect,
                HttpStatusCode.RedirectMethod,
                HttpStatusCode.SeeOther,
                HttpStatusCode.RedirectKeepVerb,
                HttpStatusCode.TemporaryRedirect
            };
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
            var uri = await GetServiceUriAsync(request, cancellationToken).ConfigureAwait(false);

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

        private async Task<Uri> GetServiceUriAsync(DaxFormatterRequest request, CancellationToken cancellationToken)
        {
            if (request is DaxFormatterMultipleRequest)
            {
                if (_daxTextFormatMultiServiceUri == null)
                    _daxTextFormatMultiServiceUri = await InitializeMultiServiceUriAsync().ConfigureAwait(false);

                return _daxTextFormatMultiServiceUri;
            }
            else if (request is DaxFormatterSingleRequest)
            {
                if (_daxTextFormatSingleServiceUri == null)
                    _daxTextFormatSingleServiceUri = await InitializeSingleServiceUriAsync().ConfigureAwait(false);

                return _daxTextFormatSingleServiceUri;
            }
            else
            {
                throw new NotSupportedException($"Uri not supported for { request.GetType().Name } request");
            }

            async Task<Uri> InitializeSingleServiceUriAsync()
            {
                if (_daxTextFormatSingleServiceUri == null)
                {
                    await _initializeServiceUriSemaphore.WaitAsync(CancellationToken.None).ConfigureAwait(false);
                    try
                    {
                        if (_daxTextFormatSingleServiceUri == null)
                        {
                            using var response = await _httpClient.GetAsync(request.DaxTextFormatUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                            var uri = request.DaxTextFormatUri;

                            if (_locationChangedStatusCodes.Contains(response.StatusCode))
                            {
                                uri = response.Headers.Location;
                            }
                            else
                            {
                                response.EnsureSuccessStatusCode();
                            }

                            _daxTextFormatSingleServiceUri = uri;
                        }
                    }
                    finally
                    {
                        _initializeServiceUriSemaphore.Release();
                    }
                }

                return _daxTextFormatSingleServiceUri;
            }

            async Task<Uri> InitializeMultiServiceUriAsync()
            {
                if (_daxTextFormatMultiServiceUri == null)
                {
                    await _initializeServiceUriSemaphore.WaitAsync(CancellationToken.None).ConfigureAwait(false);
                    try
                    {
                        if (_daxTextFormatMultiServiceUri == null)
                        {
                            using var response = await _httpClient.GetAsync(request.DaxTextFormatUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                            var uri = request.DaxTextFormatUri;

                            if (_locationChangedStatusCodes.Contains(response.StatusCode))
                            {
                                uri = response.Headers.Location;
                            }
                            else
                            {
                                response.EnsureSuccessStatusCode();
                            }

                            _daxTextFormatMultiServiceUri = uri;
                        }
                    }
                    finally
                    {
                        _initializeServiceUriSemaphore.Release();
                    }
                }

                return _daxTextFormatMultiServiceUri;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                if (disposing)
                {
                    _initializeServiceUriSemaphore.Dispose();
                    _formatSemaphore.Dispose();
                    _httpClient.Dispose();
                }
            }
        }
    }
}
