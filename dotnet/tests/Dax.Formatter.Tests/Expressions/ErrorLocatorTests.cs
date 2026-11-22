using Dax.Formatter.Expressions;
using Xunit;

namespace Dax.Formatter.Tests.Expressions;

public class ErrorLocatorTests
{
    [Theory]
    [InlineData("1 +* 2", 1, 3, 0, 3)]
    [InlineData("1 +\n2 +* 3", 2, 3, 1, 3)]
    [InlineData("1 +\r\n2 +* 3", 2, 3, 1, 3)]
    public void Locate_TextUnderAHeader_MovesTheLineUpByOne(
        string text, int line, int column, int expectedLine, int expectedColumn)
    {
        var position = ErrorLocator.Locate(text, hasHeader: true, line, column);

        Assert.Equal((expectedLine, expectedColumn), position);
    }

    [Theory]
    [InlineData("\"a => b\"", 9)]
    [InlineData("1 + 2", 0)]
    public void Locate_ErrorOnTheHeaderLine_PointsToTheStartOfTheText(string text, int column)
    {
        var position = ErrorLocator.Locate(text, hasHeader: true, line: 0, column);

        Assert.Equal((0, 0), position);
    }

    [Theory]
    [InlineData("EVALUATE Sa les", 0, 12, 0, 12)]
    [InlineData("EVALUATE\nSa les", 1, 3, 1, 3)]
    public void Locate_TextSentAsItIs_KeepsThePosition(
        string text, int line, int column, int expectedLine, int expectedColumn)
    {
        var position = ErrorLocator.Locate(text, hasHeader: false, line, column);

        Assert.Equal((expectedLine, expectedColumn), position);
    }

    [Theory]
    [InlineData("1\t+\t* 2", 16, 4)]
    [InlineData("12345678\t+* 2", 17, 10)]
    [InlineData("1234567\t+* 2", 9, 9)]
    [InlineData("\t\t* 2", 16, 2)]
    [InlineData("\t* 2", 5, 1)]
    public void Locate_LineWithTabs_CountsEachTabUpToTheNextTabStop(string text, int column, int expectedColumn)
    {
        var position = ErrorLocator.Locate(text, hasHeader: true, line: 1, column);

        Assert.Equal((0, expectedColumn), position);
    }

    [Theory]
    [InlineData("1 +\r2 +* 3", 6, 7)]
    [InlineData("1 +\r\n2", 3, 3)]
    [InlineData("1 +\r\n2", 50, 3)]
    public void Locate_LineWithCarriageReturns_GivesThemNoWidth(string text, int column, int expectedColumn)
    {
        var position = ErrorLocator.Locate(text, hasHeader: true, line: 1, column);

        Assert.Equal((0, expectedColumn), position);
    }

    [Theory]
    [InlineData("1 +", 2, 0, 3)]
    [InlineData("1 +\n", 3, 1, 0)]
    [InlineData("1 +\r\n", 3, 1, 0)]
    [InlineData("1 +\n  2 +", 9, 1, 5)]
    [InlineData("", 2, 0, 0)]
    public void Locate_LineAfterTheLastOne_PointsToTheEndOfTheText(
        string text, int line, int expectedLine, int expectedColumn)
    {
        var position = ErrorLocator.Locate(text, hasHeader: true, line, column: 0);

        Assert.Equal((expectedLine, expectedColumn), position);
    }

    [Fact]
    public void Locate_CharacterOutsideTheBasicPlane_CountsItsTwoUtf16Units()
    {
        var position = ErrorLocator.Locate("\"\U0001F600\" +* 2", hasHeader: true, line: 1, column: 6);

        Assert.Equal((0, 6), position);
    }

    [Fact]
    public void Locate_NegativePosition_PointsToTheStart()
    {
        var position = ErrorLocator.Locate("1 + 2", hasHeader: false, line: -1, column: -1);

        Assert.Equal((0, 0), position);
    }
}
