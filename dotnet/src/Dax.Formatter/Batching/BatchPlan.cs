using Dax.Formatter.Expressions;

namespace Dax.Formatter.Batching;

/// <summary>
/// How the texts of a call are sent: the header of each text, the requests they are split into, and the blank
/// texts, which are not sent.
/// </summary>
internal sealed class BatchPlan
{
    private readonly string[] _texts;
    private readonly ExpressionHeader?[]? _headers;

    public BatchPlan(string[] texts, ExpressionHeader?[]? headers, List<Chunk> chunks, List<int> blankIndexes)
    {
        _texts = texts;
        _headers = headers;
        Chunks = chunks;
        BlankIndexes = blankIndexes;
    }

    public int TextCount => _texts.Length;

    public List<Chunk> Chunks { get; }

    public List<int> BlankIndexes { get; }

    public string GetText(int index)
    {
        return _texts[index];
    }

    /// <summary>Gets the header that the text is sent under, or <see langword="null"/> when it is sent as it is.</summary>
    public ExpressionHeader? GetHeader(int index)
    {
        return _headers?[index];
    }

    /// <summary>Gets the texts of a request as they travel, each under its header.</summary>
    /// <remarks>
    /// The texts get their headers only when a request is written, so that a large call does not hold a second
    /// copy of every text.
    /// </remarks>
    public List<string> GetTextsToSend(Chunk chunk)
    {
        var dax = new List<string>(chunk.Count);
        for (var position = 0; position < chunk.Count; position++)
        {
            var index = chunk[position];
            var text = _texts[index];
            var header = GetHeader(index);
            dax.Add(header is null ? text : header.AddTo(text));
        }

        return dax;
    }
}
