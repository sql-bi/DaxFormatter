namespace Dax.Formatter;

/// <summary>
/// The result of formatting the DAX text of an item, with the item it belongs to.
/// </summary>
/// <typeparam name="TItem">The type of the item, such as a measure of a model.</typeparam>
public sealed class DaxFormatResult<TItem> : DaxFormatResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DaxFormatResult{TItem}"/> class.
    /// </summary>
    /// <param name="item">The item the text belongs to.</param>
    /// <param name="result">The result of formatting the text of the item.</param>
    public DaxFormatResult(TItem item, DaxFormatResult result)
        : base(GetOriginalText(result), result.FormattedText, result.Errors)
    {
        Item = item;
    }

    /// <summary>
    /// Gets the item the text belongs to.
    /// </summary>
    public TItem Item { get; }

    private static string GetOriginalText(DaxFormatResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.OriginalText;
    }
}
