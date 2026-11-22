using Xunit;

namespace Dax.Formatter.Tests;

public class DaxFormatResultTests
{
    private static readonly DaxSyntaxError s_error = new(0, 3, "Syntax error");

    [Fact]
    public void Success_Texts_CreatesAResultWithoutErrors()
    {
        var result = DaxFormatResult.Success("evaluate Sales", "EVALUATE\r\nSales");

        Assert.True(result.Succeeded);
        Assert.Equal(("evaluate Sales", "EVALUATE\r\nSales"), (result.OriginalText, result.FormattedText));
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Success_EmptyFormattedText_StillSucceeds()
    {
        var result = DaxFormatResult.Success(" ", string.Empty);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Failure_Errors_CreatesAResultWithoutFormattedText()
    {
        var result = DaxFormatResult.Failure("1 +* 2", [s_error]);

        Assert.False(result.Succeeded);
        Assert.Null(result.FormattedText);
        Assert.Equal([s_error], result.Errors);
    }

    [Fact]
    public void Failure_Errors_CopiesThem()
    {
        var errors = new List<DaxSyntaxError> { s_error };
        var result = DaxFormatResult.Failure("1 +* 2", errors);

        errors.Clear();

        Assert.Single(result.Errors);
    }

    [Fact]
    public void Failure_NoErrors_Throws()
    {
        Assert.Throws<ArgumentException>("errors", () => DaxFormatResult.Failure("1 +* 2", []));
    }

    [Fact]
    public void Failure_NullError_Throws()
    {
        Assert.Throws<ArgumentException>("errors", () => DaxFormatResult.Failure("1 +* 2", [null!]));
    }

    [Fact]
    public void Success_NullText_Throws()
    {
        Assert.Throws<ArgumentNullException>("originalText", () => DaxFormatResult.Success(null!, "a"));
        Assert.Throws<ArgumentNullException>("formattedText", () => DaxFormatResult.Success("a", null!));
    }

    [Fact]
    public void Constructor_ItemAndResult_CopiesTheResultAndKeepsTheItem()
    {
        var failure = DaxFormatResult.Failure("1 +* 2", [s_error]);

        var result = new DaxFormatResult<int>(42, failure);

        Assert.Equal(42, result.Item);
        Assert.Equal(("1 +* 2", false), (result.OriginalText, result.Succeeded));
        Assert.Equal([s_error], result.Errors);
    }

    [Fact]
    public void Constructor_NullResult_Throws()
    {
        Assert.Throws<ArgumentNullException>("result", () => new DaxFormatResult<int>(42, null!));
    }
}
