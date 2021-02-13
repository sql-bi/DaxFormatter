namespace Dax.Formatter.Client.Http
{
    using Dax.Formatter.Models;
    using System.Threading;
    using System.Threading.Tasks;

    internal interface IDaxFormatterHttpClient
    {
        void Dispose();

        Task<DaxFormatterSingleResponse> FormatAsync(DaxFormatterSingleRequest request, CancellationToken cancellationToken);

        Task<DaxFormatterMultipleResponse> FormatAsync(DaxFormatterMultipleRequest request, CancellationToken cancellationToken);
    }
}