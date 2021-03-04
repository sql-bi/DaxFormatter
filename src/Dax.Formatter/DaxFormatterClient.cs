namespace Dax.Formatter
{
    using Dax.Formatter.Client.Http;
    using Dax.Formatter.Models;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public class DaxFormatterClient : IDaxFormatterClient
    {
        private static readonly DaxFormatterHttpClient _formatter;
        private readonly string _application;
        private readonly string _version;

        static DaxFormatterClient()
        {
            _formatter = new DaxFormatterHttpClient();
        }

        public DaxFormatterClient()
        {
            var assembly = Assembly.GetEntryAssembly();

            _version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            _application = assembly.GetName().Name;
        }

        public DaxFormatterClient(string application, string version)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _version = version ?? throw new ArgumentNullException(nameof(version));
        }

        public async Task<DaxFormatterResponse> FormatAsync(string expression, CancellationToken cancellationToken = default)
        {
            var request = DaxFormatterSingleRequest.GetFrom(_application, _version, expression);
            var response = await _formatter.FormatAsync(request, cancellationToken).ConfigureAwait(false);

            return response;
        }

        public async Task<IReadOnlyList<DaxFormatterResponse>> FormatAsync(IEnumerable<string> expressions, CancellationToken cancellationToken = default)
        {
            var request = DaxFormatterMultipleRequest.GetFrom(_application, _version, expressions);
            var response = await _formatter.FormatAsync(request, cancellationToken).ConfigureAwait(false);

            return response;
        }

        public async Task<DaxFormatterResponse> FormatAsync(DaxFormatterSingleRequest request, CancellationToken cancellationToken = default)
        {
            request.CallerApp = _application;
            request.CallerVersion = _version;

            var response = await _formatter.FormatAsync(request, cancellationToken).ConfigureAwait(false);

            return response;
        }

        public async Task<IReadOnlyList<DaxFormatterResponse>> FormatAsync(DaxFormatterMultipleRequest request, CancellationToken cancellationToken = default)
        {
            request.CallerApp = _application;
            request.CallerVersion = _version;

            var response = await _formatter.FormatAsync(request, cancellationToken).ConfigureAwait(false);

            return response;
        }
    }
}
