namespace Dax.Formatter
{
    using Dax.Formatter.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDaxFormatterClient
    {
        Task<DaxFormatterResponse> FormatAsync(string expression, CancellationToken cancellationToken = default);

        Task<DaxFormatterResponse> FormatAsync(IEnumerable<string> expressions, CancellationToken cancellationToken = default);

        Task<DaxFormatterResponse> FormatAsync(DaxFormatterRequest request, CancellationToken cancellationToken = default);
    }
}