namespace Dax.Formatter.Expressions;

/// <summary>
/// The line that the client writes before the DAX of a model object, which has no name of its own, so that the
/// service reads it as a definition. <see cref="HeaderRemover"/> takes it away from the formatted text.
/// </summary>
internal sealed class ExpressionHeader
{
    /// <summary>The header of every expression that is not the body of a function.</summary>
    public static readonly ExpressionHeader Measure = new("[x] :=", indentsBody: false);

    /// <summary>The header of the body of a user-defined function.</summary>
    public static readonly ExpressionHeader Function = new("FUNCTION x =", indentsBody: true);

    private ExpressionHeader(string text, bool indentsBody)
    {
        Text = text;
        IndentsBody = indentsBody;
    }

    public string Text { get; }

    /// <summary>Gets a value indicating whether the service indents the lines that follow this header.</summary>
    public bool IndentsBody { get; }

    public static ExpressionHeader For(string text)
    {
        return FunctionBodyRecognizer.IsFunctionBody(text) ? Function : Measure;
    }

    // The header takes a line of its own, so that the lines of the text keep their columns.
    public string AddTo(string text)
    {
        return string.Concat(Text, "\n", text);
    }
}
