namespace Dax.Formatter.Wire;

/// <summary>
/// A syntax error in the JSON body of a response, with the position that the service reports.
/// </summary>
internal sealed class FormatResponseError
{
    public int? Line { get; set; }

    public int? Column { get; set; }

    public string? Message { get; set; }
}
