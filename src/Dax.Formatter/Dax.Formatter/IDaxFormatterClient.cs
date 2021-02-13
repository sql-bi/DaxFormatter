namespace Dax.Formatter
{
    using Dax.Formatter.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDaxFormatterClient
    {
        Task<DaxFormatterResult> FormatAsync(string expression, CancellationToken cancellationToken = default);

        Task<DaxFormatterMultipleResponse> FormatAsync(IEnumerable<string> expressions, CancellationToken cancellationToken = default);

        Task<DaxFormatterResult> FormatAsync(DaxFormatterSingleRequest request, CancellationToken cancellationToken = default);

        Task<DaxFormatterMultipleResponse> FormatAsync(DaxFormatterMultipleRequest request, CancellationToken cancellationToken = default);
    }
}