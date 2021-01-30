namespace Dax.Formatter
{
    using Dax.Formatter.Client.Http;
    using Dax.Formatter.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public static class DaxFormatterClient
    {
        private static readonly DaxFormatterHttpClient _formatter;

        static DaxFormatterClient()
        {
            System.Diagnostics.Debug.WriteLine("DAX::DaxFormatterClient.ctr");

            _formatter = new DaxFormatterHttpClient();
        }

        public static DaxFormatterResponse Format(DaxFormatterRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public static async Task<DaxFormatterResponse> FormatAsync(string expression, CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debug.WriteLine("DAX::DaxFormatterClient.FormatAsync");

            var request = DaxFormatterRequest.GetFrom(expression);

            return await _formatter.FormatAsync(request, cancellationToken);
        }

        public static async Task<DaxFormatterResponse> FormatAsync(DaxFormatterRequest request, CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debug.WriteLine("DAX::DaxFormatterClient.FormatAsync");

            return await _formatter.FormatAsync(request, cancellationToken);
        }

        public static async Task<IEnumerable<DaxFormatterResponse>> Format(IEnumerable<DaxFormatterRequest> expressions) => throw new NotImplementedException();
        
        public static async Task<IEnumerable<DaxFormatterResponse>> FormatAsync(IEnumerable<DaxFormatterRequest> expressions) => throw new NotImplementedException();
    }
}
