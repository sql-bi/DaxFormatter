namespace Dax.Formatter.Batching;

/// <summary>
/// The texts of a call that travel in one request, as their indexes in the texts of the call, in order.
/// </summary>
internal sealed class Chunk
{
    private readonly int[] _textIndexes;

    public Chunk(int[] textIndexes)
    {
        _textIndexes = textIndexes;
    }

    public int Count => _textIndexes.Length;

    public int FirstIndex => _textIndexes[0];

    public int LastIndex => _textIndexes[_textIndexes.Length - 1];

    /// <summary>Gets the index, in the texts of the call, of the text at a position of the request.</summary>
    public int this[int position] => _textIndexes[position];
}
