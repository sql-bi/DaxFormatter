namespace Dax.Formatter.Expressions;

/// <summary>
/// The pieces of a DAX text that <see cref="DaxTextScanner"/> tells apart.
/// </summary>
internal enum DaxTokenKind
{
    /// <summary>The end of the text.</summary>
    End,

    /// <summary>A line feed outside strings, names and comments.</summary>
    LineBreak,

    /// <summary>A comment that starts with <c>//</c> or <c>--</c>, without the line feed that ends it.</summary>
    LineComment,

    /// <summary>A comment between <c>/*</c> and <c>*/</c>, which can span lines.</summary>
    BlockComment,

    /// <summary>A string between double quotes, which can span lines.</summary>
    String,

    /// <summary>A table name between single quotes.</summary>
    QuotedName,

    /// <summary>A column or measure name between square brackets.</summary>
    BracketName,

    /// <summary>Any other single character.</summary>
    Other,
}
