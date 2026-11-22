namespace Dax.Formatter;

/// <summary>
/// The list separator and the decimal separator that the DAX text uses, and that the formatted text keeps.
/// </summary>
public enum DaxSeparatorStyle
{
    /// <summary>
    /// A comma between arguments and a point in decimal numbers: <c>DIVIDE ( 1.5, 2 )</c>.
    /// </summary>
    CommaAndPoint,

    /// <summary>
    /// A semicolon between arguments and a comma in decimal numbers: <c>DIVIDE ( 1,5; 2 )</c>.
    /// </summary>
    SemicolonAndComma,
}
