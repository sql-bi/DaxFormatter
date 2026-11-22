namespace Dax.Formatter;

/// <summary>
/// Says what a DAX text is, so that the client knows how to send it to the service.
/// </summary>
public enum DaxKind
{
    /// <summary>
    /// The text is sent as it is. Use it for a query, such as <c>EVALUATE Sales</c>, and for a text that
    /// carries its own name, such as <c>[Margin] := [Sales] - [Cost]</c>.
    /// </summary>
    Auto,

    /// <summary>
    /// The text is the DAX of one model object, without its name: a measure, a calculated column, a calculated
    /// table, a calculation item or the body of a user-defined function.
    /// </summary>
    Expression,
}
