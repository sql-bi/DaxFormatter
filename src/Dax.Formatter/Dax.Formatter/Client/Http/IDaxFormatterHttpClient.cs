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

        Task<DaxFormatterMultipleResponse> FormatAsync(DaxFormatterMultipleRequest request, CancellationToken cancellationToken);

        Task<IEnumerable<DaxFormatterMultipleResponse>> FormatAsync(IEnumerable<DaxFormatterMultipleRequest> requests, CancellationToken cancellationToken);
    }
}