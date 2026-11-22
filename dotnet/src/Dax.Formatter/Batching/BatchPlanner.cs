using Dax.Formatter.Configuration;
using Dax.Formatter.Expressions;

namespace Dax.Formatter.Batching;

/// <summary>
/// Splits the texts of a call into requests that stay within the limits, and chooses the header of each text.
/// </summary>
internal static class BatchPlanner
{
    public static BatchPlan Plan(string[] texts, DaxKind kind, BatchLimits limits)
    {
        var headers = kind == DaxKind.Expression ? new ExpressionHeader?[texts.Length] : null;
        var chunks = new List<Chunk>();
        var blankIndexes = new List<int>();
        var current = new List<int>();
        var currentCharacters = 0;

        for (var index = 0; index < texts.Length; index++)
        {
            var text = texts[index];
            if (string.IsNullOrWhiteSpace(text))
            {
                blankIndexes.Add(index);
                continue;
            }

            if (headers is not null)
            {
                headers[index] = ExpressionHeader.For(text);
            }

            // A text longer than the limit still travels, in a request of its own.
            var isFull = current.Count >= limits.MaxTexts || currentCharacters + text.Length > limits.MaxCharacters;
            if (current.Count > 0 && isFull)
            {
                chunks.Add(new Chunk([.. current]));
                current.Clear();
                currentCharacters = 0;
            }

            current.Add(index);
            currentCharacters += text.Length;
        }

        if (current.Count > 0)
        {
            chunks.Add(new Chunk([.. current]));
        }

        return new BatchPlan(texts, headers, chunks, blankIndexes);
    }
}
