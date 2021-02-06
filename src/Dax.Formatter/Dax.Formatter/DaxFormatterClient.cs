namespace Dax.Formatter
{
    using Dax.Formatter.Client.Http;
    using Dax.Formatter.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public static class DaxFormatterClient
    {
        private static readonly DaxFormatterHttpClient _formatter;

        static DaxFormatterClient()
        {
            _formatter = new DaxFormatterHttpClient();
        }

        public static async Task<DaxFormatterResponse> FormatAsync(string expression, CancellationToken cancellationToken = default)
        {
            var expressions = new List<string>
            {
                expression
            };

            var response = await FormatAsync(expressions, cancellationToken).ConfigureAwait(false);

            return response;
        }

        public static async Task<DaxFormatterResponse> FormatAsync(IEnumerable<string> expressions, CancellationToken cancellationToken = default)
        {
            var request = DaxFormatterRequest.GetFrom(expressions);

            var response = await _formatter.FormatAsync(request, cancellationToken).ConfigureAwait(false);

            return response;
        }
    }
}
