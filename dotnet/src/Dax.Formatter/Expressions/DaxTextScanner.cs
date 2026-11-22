namespace Dax.Formatter.Expressions;

/// <summary>
/// Reads a DAX text one piece at a time, only as far as needed to know where strings, names and comments start
/// and end. It never fails: a piece that is not closed lasts until the end of the text.
/// </summary>
internal sealed class DaxTextScanner
{
    private readonly string _text;

    public DaxTextScanner(string text, int position)
    {
        _text = text;
        Position = position;
        TokenStart = position;
    }

    /// <summary>Gets the index of the first character that was not read yet.</summary>
    public int Position { get; private set; }

    /// <summary>Gets the index of the first character of the piece that <see cref="Read"/> returned last.</summary>
    public int TokenStart { get; private set; }

    /// <summary>Gets the first character of the piece that <see cref="Read"/> returned last.</summary>
    public char TokenCharacter => _text[TokenStart];

    public DaxTokenKind Read()
    {
        TokenStart = Position;
        if (Position >= _text.Length)
        {
            return DaxTokenKind.End;
        }

        var character = _text[Position];
        switch (character)
        {
            case '\n':
                Position++;
                return DaxTokenKind.LineBreak;
            case '"':
                Position = FindEndOfDelimited('"');
                return DaxTokenKind.String;
            case '\'':
                Position = FindEndOfDelimited('\'');
                return DaxTokenKind.QuotedName;
            case '[':
                Position = FindEndOfDelimited(']');
                return DaxTokenKind.BracketName;
            case '/' when NextIs('/'):
            case '-' when NextIs('-'):
                Position = FindEndOfLine();
                return DaxTokenKind.LineComment;
            case '/' when NextIs('*'):
                Position = FindEndOfBlockComment();
                return DaxTokenKind.BlockComment;
            default:
                Position++;
                return DaxTokenKind.Other;
        }
    }

    /// <summary>Moves past characters that the caller has already examined.</summary>
    public void Skip(int count)
    {
        Position += count;
    }

    private bool NextIs(char character)
    {
        return Position + 1 < _text.Length && _text[Position + 1] == character;
    }

    // The closing character written twice stands for itself and does not close the piece.
    private int FindEndOfDelimited(char closing)
    {
        var index = Position + 1;
        while (index < _text.Length)
        {
            if (_text[index] != closing)
            {
                index++;
            }
            else if (index + 1 < _text.Length && _text[index + 1] == closing)
            {
                index += 2;
            }
            else
            {
                return index + 1;
            }
        }

        return _text.Length;
    }

    private int FindEndOfLine()
    {
        var lineFeed = _text.IndexOf('\n', Position);
        return lineFeed < 0 ? _text.Length : lineFeed;
    }

    private int FindEndOfBlockComment()
    {
        var end = _text.IndexOf("*/", Position + 2, StringComparison.Ordinal);
        return end < 0 ? _text.Length : end + 2;
    }
}
