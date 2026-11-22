using System.Diagnostics.CodeAnalysis;

namespace Dax.Formatter;

/// <summary>
/// The result of formatting one DAX text: the formatted text, or the syntax errors that the service found.
/// </summary>
public class DaxFormatResult
{
    private static readonly IReadOnlyList<DaxSyntaxError> s_noErrors = [];

    private protected DaxFormatResult(string originalText, string? formattedText, IReadOnlyList<DaxSyntaxError> errors)
    {
        OriginalText = originalText;
        FormattedText = formattedText;
        Errors = errors;
    }

    /// <summary>
    /// Gets the text that was given to the client.
    /// </summary>
    public string OriginalText { get; }

    /// <summary>
    /// Gets a value indicating whether the text was formatted, which means that it has no syntax errors.
    /// </summary>
    [MemberNotNullWhen(true, nameof(FormattedText))]
    public bool Succeeded => FormattedText is not null;

    /// <summary>
    /// Gets the formatted text, without a final line break, or <see langword="null"/> when
    /// <see cref="Succeeded"/> is <see langword="false"/>.
    /// </summary>
    public string? FormattedText { get; }

    /// <summary>
    /// Gets the syntax errors of the text. The list is empty when <see cref="Succeeded"/> is
    /// <see langword="true"/>.
    /// </summary>
    public IReadOnlyList<DaxSyntaxError> Errors { get; }

    /// <summary>
    /// Creates the result of a text that was formatted.
    /// </summary>
    /// <param name="originalText">The text that was given to the client.</param>
    /// <param name="formattedText">The formatted text.</param>
    /// <returns>A result whose <see cref="Succeeded"/> is <see langword="true"/>.</returns>
    public static DaxFormatResult Success(string originalText, string formattedText)
    {
        ArgumentNullException.ThrowIfNull(originalText);
        ArgumentNullException.ThrowIfNull(formattedText);

        return new DaxFormatResult(originalText, formattedText, s_noErrors);
    }

    /// <summary>
    /// Creates the result of a text that has syntax errors.
    /// </summary>
    /// <param name="originalText">The text that was given to the client.</param>
    /// <param name="errors">The syntax errors of the text: at least one.</param>
    /// <returns>A result whose <see cref="Succeeded"/> is <see langword="false"/>.</returns>
    public static DaxFormatResult Failure(string originalText, IEnumerable<DaxSyntaxError> errors)
    {
        ArgumentNullException.ThrowIfNull(originalText);
        ArgumentNullException.ThrowIfNull(errors);

        DaxSyntaxError[] copy = [.. errors];
        if (copy.Length == 0)
        {
            throw new ArgumentException("A failure needs at least one syntax error.", nameof(errors));
        }

        if (Array.IndexOf(copy, null) >= 0)
        {
            throw new ArgumentException("A syntax error is null.", nameof(errors));
        }

        return new DaxFormatResult(originalText, formattedText: null, Array.AsReadOnly(copy));
    }
}
