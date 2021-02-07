namespace Dax.Formatter
{
    using Dax.Formatter.Client.Http;
    using Dax.Formatter.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class DaxFormatterClient : IDaxFormatterClient
    {
        private static readonly DaxFormatterHttpClient _formatter;

        static DaxFormatterClient()
        {
            _formatter = new DaxFormatterHttpClient();
        }

        public async Task<DaxFormatterResult> FormatAsync(string expression, CancellationToken cancellationToken = default)
        {
            var request = DaxFormatterRequestBase.GetFrom(expression);

            var response = await _formatter.FormatAsync(request, cancellationToken).ConfigureAwait(false);

            return response;
        }

        public async Task<DaxFormatterResponse> FormatAsync(IEnumerable<string> expressions, CancellationToken cancellationToken = default)
        {
            var request = DaxFormatterRequestBase.GetFrom(expressions);

            var response = await _formatter.FormatAsync(request, cancellationToken).ConfigureAwait(false);

            return response;
        }

        public async Task<DaxFormatterResult> FormatAsync(DaxFormatterSingleRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _formatter.FormatAsync(request, cancellationToken).ConfigureAwait(false);
            return response;
        }

        public async Task<DaxFormatterResponse> FormatAsync(DaxFormatterMultipleRequests request, CancellationToken cancellationToken = default)
        {
            var response = await _formatter.FormatAsync(request, cancellationToken).ConfigureAwait(false);
            return response;
        }
    }
}
