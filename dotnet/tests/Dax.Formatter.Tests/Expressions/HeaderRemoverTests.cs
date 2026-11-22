using Dax.Formatter.Expressions;
using Xunit;

namespace Dax.Formatter.Tests.Expressions;

public class HeaderRemoverTests
{
    [Theory]
    [InlineData("[x] :=\r\n1 + 2\r\n", "1 + 2\r\n")]
    [InlineData("[x] :=\n1 + 2\n", "1 + 2\n")]
    [InlineData("[x] := 1 + 2\r\n", "1 + 2\r\n")]
    [InlineData("[x] :=\r\n\r\n1 + 2", "\r\n1 + 2")]
    [InlineData("[x] :=\r\n    IF (\r\n        1\r\n    )", "    IF (\r\n        1\r\n    )")]
    [InlineData("[x] :=", "")]
    public void TryRemove_MeasureHeader_ReturnsWhatFollowsIt(string formatted, string expected)
    {
        var removed = HeaderRemover.TryRemove(formatted, ExpressionHeader.Measure, out var body);

        Assert.True(removed);
        Assert.Equal(expected, body);
    }

    [Theory]
    [InlineData(
        "FUNCTION x = (\r\n        a,\r\n        b\r\n    ) =>\r\n    a + b\r\n",
        "(\r\n    a,\r\n    b\r\n) =>\r\na + b\r\n")]
    [InlineData(
        "FUNCTION x =\r\n    // c\r\n    (\r\n        a\r\n    ) =>\r\n    a\r\n",
        "// c\r\n(\r\n    a\r\n) =>\r\na\r\n")]
    [InlineData("FUNCTION x = (\n        a\n    ) =>\n    a", "(\n    a\n) =>\na")]
    public void TryRemove_FunctionHeader_RemovesTheIndentationToo(string formatted, string expected)
    {
        var removed = HeaderRemover.TryRemove(formatted, ExpressionHeader.Function, out var body);

        Assert.True(removed);
        Assert.Equal(expected, body);
    }

    [Theory]
    [InlineData(
        "FUNCTION x = (\r\n        a\r\n    ) =>\r\n    \"x\n        y\" & a\r\n",
        "(\r\n    a\r\n) =>\r\n\"x\n        y\" & a\r\n")]
    [InlineData(
        "FUNCTION x =\r\n    /* c1\n    c2 */\r\n    (\r\n        a\r\n    ) =>\r\n    a\r\n",
        "/* c1\n    c2 */\r\n(\r\n    a\r\n) =>\r\na\r\n")]
    [InlineData(
        "FUNCTION x = (\r\n        a\r\n    ) =>\r\n    'T\n    1'[c\n    d] & a\r\n",
        "(\r\n    a\r\n) =>\r\n'T\n    1'[c\n    d] & a\r\n")]
    [InlineData(
        "FUNCTION x = (\r\n        a\r\n    ) =>\r\n    a // \"\r\n    + 1 -- /*\r\n    + 2\r\n",
        "(\r\n    a\r\n) =>\r\na // \"\r\n+ 1 -- /*\r\n+ 2\r\n")]
    public void TryRemove_LineThatStartsInsideAStringOrComment_KeepsItsSpaces(string formatted, string expected)
    {
        var removed = HeaderRemover.TryRemove(formatted, ExpressionHeader.Function, out var body);

        Assert.True(removed);
        Assert.Equal(expected, body);
    }

    [Fact]
    public void TryRemove_LineWithLessIndentation_LeavesItAsItIs()
    {
        var removed = HeaderRemover.TryRemove("FUNCTION x = (\r\n  a\r\n\r\n) => a", ExpressionHeader.Function, out var body);

        Assert.True(removed);
        Assert.Equal("(\r\n  a\r\n\r\n) => a", body);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1 + 2\r\n")]
    [InlineData(" [x] :=\r\n1 + 2")]
    [InlineData("[X] :=\r\n1 + 2")]
    [InlineData("FUNCTION x = (\r\n        a\r\n    ) =>\r\n    a\r\n")]
    public void TryRemove_HeaderMissing_ReturnsFalse(string formatted)
    {
        var removed = HeaderRemover.TryRemove(formatted, ExpressionHeader.Measure, out var body);

        Assert.False(removed);
        Assert.Null(body);
    }
}
