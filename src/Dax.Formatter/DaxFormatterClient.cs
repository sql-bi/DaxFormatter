namespace Dax.Formatter
{
    using Dax.Formatter.Client.Http;
    using Dax.Formatter.Models;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public class DaxFormatterClient : IDaxFormatterClient
    {
        private readonly DaxFormatterHttpClient _formatter;
        private readonly string? _application;
        private readonly string? _version;

        public DaxFormatterClient(string? application = null, string? version = null)
        {
            if (application == null || version == null)
            {
                var assembly = Assembly.GetEntryAssembly();
                if (assembly != null)
                {
                    var assemblyName = assembly.GetName();
                    var assemblyAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();

                    version = assemblyAttribute.Version;
                    application = assemblyName.Name;
                }
            }

            _application = application;
            _version = version;
            _formatter = new DaxFormatterHttpClient();
        }

        public async Task<DaxFormatterResponse?> FormatAsync(string expression, CancellationToken cancellationToken = default)
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

        public async Task<DaxFormatterResponse?> FormatAsync(DaxFormatterSingleRequest request, CancellationToken cancellationToken = default)
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
