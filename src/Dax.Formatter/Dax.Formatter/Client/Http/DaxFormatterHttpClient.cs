namespace Dax.Formatter.Client.Http
{
    using Dax.Formatter.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    internal class DaxFormatterHttpClient : IDaxFormatterHttpClient, IDisposable
    {
        private const int DaxFormatterTimeoutSeconds = 60;
        private const string DaxTextFormatSingleUri = "https://www.daxformatter.com/api/daxformatter/daxtextformat";
        private const string DaxTextFormatMultiUri = "https://www.daxformatter.com/api/daxformatter/daxtextformatmulti";
        private const string MediaTypeNamesApplicationJson = "application/json";

        private readonly HashSet<HttpStatusCode> _locationChangedHttpStatusCodes;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphoreSingle;
        private readonly SemaphoreSlim _semaphoreMulti;
        private Uri _daxTextFormatSingleServiceUri;
        private Uri _daxTextFormatMultiServiceUri;
        private bool _disposed;

        protected async Task<Uri> GetServiceUri( DaxFormatterRequest request, CancellationToken cancellationToken )
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
                throw new NotSupportedException($"Uri not supported for {request.GetType().Name} request");
            }

            async Task InitializeSingleServiceUriAsync()
            {
                if (_daxTextFormatSingleServiceUri == default)
                {
                    await _semaphoreSingle.WaitAsync();
                    try
                    {
                        if (_daxTextFormatSingleServiceUri == default)
                        {
                            System.Diagnostics.Debug.WriteLine("DAX::DaxFormatterClient.FormatAsync.InitializeSingleServiceUriAsync");

                            using (var response = await _httpClient.GetAsync(DaxTextFormatSingleUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                            {
                                var uri = _locationChangedHttpStatusCodes.Contains(response.StatusCode) ? response.Headers.Location : new Uri(DaxTextFormatSingleUri);
                                Interlocked.CompareExchange(ref _daxTextFormatSingleServiceUri, uri, default);
                            }
                        }
                    }
                    finally
                    {
                        _semaphoreSingle.Release();
                    }
                }
            }

            async Task InitializeMultiServiceUriAsync()
            {
                if (_daxTextFormatMultiServiceUri == default)
                {
                    await _semaphoreMulti.WaitAsync();
                    try
                    {
                        if (_daxTextFormatMultiServiceUri == default)
                        {
                            System.Diagnostics.Debug.WriteLine("DAX::DaxFormatterClient.FormatAsync.InitializeMultiServiceUriAsync");

                            using (var response = await _httpClient.GetAsync(DaxTextFormatMultiUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                            {
                                var uri = _locationChangedHttpStatusCodes.Contains(response.StatusCode) ? response.Headers.Location : new Uri(DaxTextFormatMultiUri);
                                Interlocked.CompareExchange(ref _daxTextFormatMultiServiceUri, uri, default);
                            }
                        }
                    }
                    finally
                    {
                        _semaphoreMulti.Release();
                    }
                }
            }
        }

        public DaxFormatterHttpClient()
        {
            var handler = new DaxFormatterHttpClientMessageHandler();

            _httpClient = new HttpClient(handler, disposeHandler: true);
            _httpClient.Timeout = TimeSpan.FromSeconds(DaxFormatterTimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNamesApplicationJson));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue(nameof(DecompressionMethods.GZip)));

            _semaphoreSingle = new SemaphoreSlim(1);
            _semaphoreMulti = new SemaphoreSlim(1);

            _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                IgnoreNullValues = true
            };

            _locationChangedHttpStatusCodes = new HashSet<HttpStatusCode>
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

        public async Task<DaxFormatterResult> FormatAsync(DaxFormatterSingleRequest request, CancellationToken cancellationToken)
        {
            string message = await FormatAsyncInternal(request, cancellationToken);
            var result = JsonSerializer.Deserialize<DaxFormatterResult>(message, _serializerOptions);
            return result;
        }

        public async Task<DaxFormatterResponse> FormatAsync(DaxFormatterMultipleRequest request, CancellationToken cancellationToken)
        {
            string message = await FormatAsyncInternal(request, cancellationToken);
            var result = JsonSerializer.Deserialize<DaxFormatterResponse>(message, _serializerOptions);
            return result;
        }

        private async Task<string> FormatAsyncInternal<T>(T request, CancellationToken cancellationToken) where T : DaxFormatterRequest
        {
            if (cancellationToken.IsCancellationRequested)
                return default;

            var json = JsonSerializer.Serialize(request, _serializerOptions);
            var serviceUri = await GetServiceUri(request, cancellationToken);
            
            using (var content = new StringContent(json, Encoding.UTF8, MediaTypeNamesApplicationJson))
            using (var response = await _httpClient.PostAsync(serviceUri, content, cancellationToken))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            {
                var message = reader.ReadToEnd();
                System.Diagnostics.Debug.WriteLine($"DAX::DaxFormatterClient.FormatAsync({ message })");
                return message;
            }
        }

        public async Task<IEnumerable<DaxFormatterResponse>> FormatAsync(IEnumerable<DaxFormatterMultipleRequest> requests, CancellationToken cancellationToken)
        {
            var tasks = requests.Select((r) => FormatAsync(r, cancellationToken));
            var responses = await Task.WhenAll(tasks);

            return responses;
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
                    _semaphoreSingle.Dispose();
                    _semaphoreMulti.Dispose();
                    _httpClient.Dispose();
                }
            }
        }
    }
}
