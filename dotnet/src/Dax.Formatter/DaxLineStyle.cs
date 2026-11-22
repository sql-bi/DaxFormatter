namespace Dax.Formatter;

/// <summary>
/// The length of the lines in the formatted text.
/// </summary>
public enum DaxLineStyle
{
    /// <summary>
    /// Long lines: an expression stays on one line while it fits.
    /// </summary>
    LongLine,

    /// <summary>
    /// Short lines: each argument of a function goes on a line of its own.
    /// </summary>
    ShortLine,
}
