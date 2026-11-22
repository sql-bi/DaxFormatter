namespace Dax.Formatter;

/// <summary>
/// A syntax error that the service found in a DAX text, with its position in that text.
/// </summary>
public sealed class DaxSyntaxError : IEquatable<DaxSyntaxError>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DaxSyntaxError"/> class.
    /// </summary>
    /// <param name="line">The zero-based line of the error.</param>
    /// <param name="column">The zero-based column of the error.</param>
    /// <param name="message">The description of the error.</param>
    public DaxSyntaxError(int line, int column, string message)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(line);
        ArgumentOutOfRangeException.ThrowIfNegative(column);
        ArgumentNullException.ThrowIfNull(message);

        Line = line;
        Column = column;
        Message = message;
    }

    /// <summary>
    /// Gets the zero-based line of the error in <see cref="DaxFormatResult.OriginalText"/>, where lines are
    /// separated by a line feed.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Gets the zero-based column of the error: an index into the UTF-16 characters of the line. It equals the
    /// length of the line when the error is at its end.
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// Gets the description of the error, as the service wrote it.
    /// </summary>
    public string Message { get; }

    /// <inheritdoc/>
    public bool Equals(DaxSyntaxError? other)
    {
        if (other is null)
        {
            return false;
        }

        return Line == other.Line
            && Column == other.Column
            && string.Equals(Message, other.Message, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as DaxSyntaxError);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = (Line * 397) ^ Column;
            return (hash * 397) ^ StringComparer.Ordinal.GetHashCode(Message);
        }
    }
}
