namespace Dax.Formatter
{
    using Dax.Formatter.Client.Http;
    using Dax.Formatter.Models;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class DaxFormatterClient : IDaxFormatterClient, IDisposable
    {
        private readonly DaxFormatterHttpClient _formatter;

        public DaxFormatterClient(string? application = null, string? version = null)
        {
            if (application == null || version == null)
            {
                var assembly = Assembly.GetEntryAssembly();
                if (assembly != null)
                {
                    var assemblyName = assembly.GetName();
                    var assemblyAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();

                    if (version == null)
                        version = assemblyAttribute?.Version;

                    if (application == null)
                        application = assemblyName.Name;
                }
            }

            _formatter = new DaxFormatterHttpClient(application, version);
        }

        public async Task<DaxFormatterResponse?> FormatAsync(string expression, CancellationToken cancellationToken = default)
        {
            var request = DaxFormatterSingleRequest.CreateFrom(expression);
            var response = await _formatter.FormatAsync(request, cancellationToken).ConfigureAwait(false);

            return response;
        }

        public async Task<IReadOnlyList<DaxFormatterResponse>> FormatAsync(IEnumerable<string> expressions, CancellationToken cancellationToken = default)
        {
            var request = DaxFormatterMultipleRequest.CreateFrom(expressions);
            var response = await _formatter.FormatAsync(request, cancellationToken).ConfigureAwait(false);

            return response;
        }

        public async Task<DaxFormatterResponse?> FormatAsync(DaxFormatterSingleRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _formatter.FormatAsync(request, cancellationToken).ConfigureAwait(false);
            return response;
        }

        public async Task<IReadOnlyList<DaxFormatterResponse>> FormatAsync(DaxFormatterMultipleRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _formatter.FormatAsync(request, cancellationToken).ConfigureAwait(false);
            return response;
        }

        #region IDisposable

        public void Dispose()
        {
            _formatter.Dispose();
        }

        #endregion
    }
}
