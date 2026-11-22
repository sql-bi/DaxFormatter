using Dax.Formatter.Expressions;
using Xunit;

namespace Dax.Formatter.Tests.Expressions;

public class ExpressionHeaderTests
{
    [Fact]
    public void For_FunctionBody_ReturnsTheFunctionHeader()
    {
        var header = ExpressionHeader.For("(a, b) => a + b");

        Assert.Same(ExpressionHeader.Function, header);
    }

    [Theory]
    [InlineData("SUM(Sales[Amount])")]
    [InlineData("(1 + 2) * 3")]
    public void For_OtherExpression_ReturnsTheMeasureHeader(string text)
    {
        var header = ExpressionHeader.For(text);

        Assert.Same(ExpressionHeader.Measure, header);
    }

    [Fact]
    public void AddTo_Measure_PutsTheHeaderOnALineOfItsOwn()
    {
        var sent = ExpressionHeader.Measure.AddTo("1 +\r\n2");

        Assert.Equal("[x] :=\n1 +\r\n2", sent);
    }

    [Fact]
    public void AddTo_Function_PutsTheHeaderOnALineOfItsOwn()
    {
        var sent = ExpressionHeader.Function.AddTo("(a) => a");

        Assert.Equal("FUNCTION x =\n(a) => a", sent);
    }
}
