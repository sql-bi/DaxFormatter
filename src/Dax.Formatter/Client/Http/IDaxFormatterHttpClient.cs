namespace Dax.Formatter.Client.Http
{
    using Dax.Formatter.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal interface IDaxFormatterHttpClient
    {
        void Dispose();

        Task<DaxFormatterResponse?> FormatAsync(DaxFormatterSingleRequest request, CancellationToken cancellationToken);

        Task<IReadOnlyList<DaxFormatterResponse>> FormatAsync(DaxFormatterMultipleRequest request, CancellationToken cancellationToken);
    }
}