namespace Dax.Formatter;

/// <summary>
/// The options of one formatting call. Every property has a default, so an instance is needed only to change one.
/// </summary>
public sealed class DaxFormatOptions
{
    /// <summary>
    /// Gets or sets what the texts are. The default is <see cref="DaxKind.Auto"/>.
    /// </summary>
    public DaxKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the length of the lines. The default is <see cref="DaxLineStyle.LongLine"/>.
    /// </summary>
    public DaxLineStyle LineStyle { get; set; }

    /// <summary>
    /// Gets or sets the spacing after a function name. The default is
    /// <see cref="DaxSpacingStyle.SpaceAfterFunction"/>.
    /// </summary>
    public DaxSpacingStyle SpacingStyle { get; set; }

    /// <summary>
    /// Gets or sets the list and decimal separators that the texts use. The default is
    /// <see cref="DaxSeparatorStyle.CommaAndPoint"/>.
    /// </summary>
    public DaxSeparatorStyle SeparatorStyle { get; set; }

    /// <summary>
    /// Gets or sets the description of the model the texts come from. It is optional and does not change the
    /// formatting: the service uses it for anonymous usage statistics.
    /// </summary>
    public DaxModelInfo? ModelInfo { get; set; }
}
