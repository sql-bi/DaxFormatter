namespace Dax.Formatter.Client.Http
{
    using Dax.Formatter.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal interface IDaxFormatterHttpClient
    {
        void Dispose();

        Task<DaxFormatterResult> FormatAsync(DaxFormatterSingleRequest request, CancellationToken cancellationToken);
        Task<DaxFormatterResponse> FormatAsync(DaxFormatterMultipleRequests request, CancellationToken cancellationToken);

        Task<IEnumerable<DaxFormatterResponse>> FormatAsync(IEnumerable<DaxFormatterMultipleRequests> requests, CancellationToken cancellationToken);
    }
}