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

        private Uri _daxTextFormatSingleServiceUri;
        private Uri _daxTextFormatMultiServiceUri;
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
            
            _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                IgnoreNullValues = true
            };
            
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

        public async Task<DaxFormatterResponse> FormatAsync(DaxFormatterSingleRequest request, CancellationToken cancellationToken)
        {
            await _formatSemaphore.WaitAsync();
            try
            {
                var message = await FormatAsyncInternal(request, cancellationToken);
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
            await _formatSemaphore.WaitAsync();
            try
            {
                var message = await FormatAsyncInternal(request, cancellationToken);
                var result = JsonSerializer.Deserialize<IReadOnlyList<DaxFormatterResponse>>(message, _serializerOptions);

                return result;
            }
            finally
            {
                _formatSemaphore.Release();
            }
        }

        private async Task<string> FormatAsyncInternal<T>(T request, CancellationToken cancellationToken) where T : DaxFormatterRequest
        {
            if (cancellationToken.IsCancellationRequested)
                return default;

            var json = JsonSerializer.Serialize(request, _serializerOptions);
            var uri = await GetServiceUri(request, cancellationToken);
            
            using (var content = new StringContent(json, Encoding.UTF8, MediaTypeNamesApplicationJson))
            using (var response = await _httpClient.PostAsync(uri, content, cancellationToken))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            {
                var message = reader.ReadToEnd();
                return message;
            }
        }

        private async Task<Uri> GetServiceUri(DaxFormatterRequest request, CancellationToken cancellationToken)
        {
            if (request is DaxFormatterMultipleRequest)
            {
                if (_daxTextFormatMultiServiceUri == default)
                    await InitializeMultiServiceUriAsync();

                return _daxTextFormatMultiServiceUri;
            }
            else if (request is DaxFormatterSingleRequest)
            {
                if (_daxTextFormatSingleServiceUri == default)
                    await InitializeSingleServiceUriAsync();

                return _daxTextFormatSingleServiceUri;
            }
            else
            {
                throw new NotSupportedException($"Uri not supported for { request.GetType().Name } request");
            }

            async Task InitializeSingleServiceUriAsync()
            {
                if (_daxTextFormatSingleServiceUri == default)
                {
                    await _initializeServiceUriSemaphore.WaitAsync();
                    try
                    {
                        if (_daxTextFormatSingleServiceUri == default)
                        {
                            using (var response = await _httpClient.GetAsync(request.DaxTextFormatUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                            {
                                var uri = _locationChangedStatusCodes.Contains(response.StatusCode) ? response.Headers.Location : request.DaxTextFormatUri;
                                Interlocked.CompareExchange(ref _daxTextFormatSingleServiceUri, uri, default);
                            }
                        }
                    }
                    finally
                    {
                        _initializeServiceUriSemaphore.Release();
                    }
                }
            }

            async Task InitializeMultiServiceUriAsync()
            {
                if (_daxTextFormatMultiServiceUri == default)
                {
                    await _initializeServiceUriSemaphore.WaitAsync();
                    try
                    {
                        if (_daxTextFormatMultiServiceUri == default)
                        {
                            using (var response = await _httpClient.GetAsync(request.DaxTextFormatUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                            {
                                var uri = _locationChangedStatusCodes.Contains(response.StatusCode) ? response.Headers.Location : request.DaxTextFormatUri;
                                Interlocked.CompareExchange(ref _daxTextFormatMultiServiceUri, uri, default);
                            }
                        }
                    }
                    finally
                    {
                        _initializeServiceUriSemaphore.Release();
                    }
                }
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
