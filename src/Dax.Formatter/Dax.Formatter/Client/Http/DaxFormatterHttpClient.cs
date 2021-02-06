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
        private const string DaxTextFormatUri = "https://www.daxformatter.com/api/daxformatter/daxtextformatmulti";
        private const string MediaTypeNamesApplicationJson = "application/json";

        private readonly HashSet<HttpStatusCode> _locationChangedHttpStatusCodes;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly SemaphoreSlim _semaphore;
        private readonly HttpClient _httpClient;        
        private Uri _daxTextFormatServiceUri;
        private bool _disposed;

        public DaxFormatterHttpClient()
        {
            var handler = new DaxFormatterHttpClientMessageHandler();

            _httpClient = new HttpClient(handler, disposeHandler: true);
            _httpClient.Timeout = TimeSpan.FromSeconds(DaxFormatterTimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNamesApplicationJson));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue(nameof(DecompressionMethods.GZip)));

            _semaphore = new SemaphoreSlim(1);

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

        public async Task<DaxFormatterResponse> FormatAsync(DaxFormatterRequest request, CancellationToken cancellationToken)
        {
            if (_daxTextFormatServiceUri == default)
                await InitializeUriAsync();

            if (cancellationToken.IsCancellationRequested)
                return default;

            var json = JsonSerializer.Serialize(request, _serializerOptions);

            using (var content = new StringContent(json, Encoding.UTF8, MediaTypeNamesApplicationJson))
            using (var response = await _httpClient.PostAsync(_daxTextFormatServiceUri, content, cancellationToken))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            {
                var message = reader.ReadToEnd();
                System.Diagnostics.Debug.WriteLine($"DAX::DaxFormatterClient.FormatAsync({ message })");
                var result = JsonSerializer.Deserialize<DaxFormatterResponse>(message, _serializerOptions);

                return result;
            }

            async Task InitializeUriAsync()
            {
                if (_daxTextFormatServiceUri == default)
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        if (_daxTextFormatServiceUri == default)
                        {
                            System.Diagnostics.Debug.WriteLine("DAX::DaxFormatterClient.FormatAsync.InitializeUriAsync");

                            using (var response = await _httpClient.GetAsync(DaxTextFormatUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                            {
                                var uri = _locationChangedHttpStatusCodes.Contains(response.StatusCode) ? response.Headers.Location : new Uri(DaxTextFormatUri);
                                Interlocked.CompareExchange(ref _daxTextFormatServiceUri, uri, default);
                            }
                        }
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
            }
        }

        public async Task<IEnumerable<DaxFormatterResponse>> FormatAsync(IEnumerable<DaxFormatterRequest> requests, CancellationToken cancellationToken)
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
                    _semaphore.Dispose();
                    _httpClient.Dispose();
                }
            }
        }
    }
}
