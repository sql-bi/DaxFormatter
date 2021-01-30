namespace Dax.Formatter.Client.Http
{
    using Dax.Formatter.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal interface IDaxFormatterHttpClient
    {
        void Dispose();

        Task<DaxFormatterResponse> FormatAsync(DaxFormatterRequest request, CancellationToken cancellationToken);

        IAsyncEnumerable<DaxFormatterResponse> FormatAsync(IEnumerable<DaxFormatterRequest> requests, CancellationToken cancellationToken);
    }
}