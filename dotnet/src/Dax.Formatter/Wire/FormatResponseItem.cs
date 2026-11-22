namespace Dax.Formatter.Wire;

/// <summary>
/// The result of one text in the JSON body of a response. The service can leave any field out or null.
/// </summary>
internal sealed class FormatResponseItem
{
    public string? Formatted { get; set; }

    public List<FormatResponseError?>? Errors { get; set; }
}
