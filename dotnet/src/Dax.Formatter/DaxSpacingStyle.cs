namespace Dax.Formatter;

/// <summary>
/// The spacing between a function name and its opening parenthesis in the formatted text.
/// </summary>
public enum DaxSpacingStyle
{
    /// <summary>
    /// A space after the function name: <c>SUM ( Sales[Amount] )</c>.
    /// </summary>
    SpaceAfterFunction,

    /// <summary>
    /// No space after the function name: <c>SUM( Sales[Amount] )</c>.
    /// </summary>
    NoSpaceAfterFunction,
}
