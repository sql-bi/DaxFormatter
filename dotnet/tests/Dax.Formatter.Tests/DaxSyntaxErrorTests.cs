using Xunit;

namespace Dax.Formatter.Tests;

public class DaxSyntaxErrorTests
{
    [Fact]
    public void Constructor_Values_KeepsThem()
    {
        var error = new DaxSyntaxError(2, 7, "Syntax error");

        Assert.Equal((2, 7, "Syntax error"), (error.Line, error.Column, error.Message));
    }

    [Fact]
    public void Equals_SameValues_IsTrueWithTheSameHashCode()
    {
        var first = new DaxSyntaxError(2, 7, "Syntax error");
        var second = new DaxSyntaxError(2, 7, "Syntax error");

        Assert.True(first.Equals(second));
        Assert.True(first.Equals((object)second));
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Theory]
    [InlineData(3, 7, "Syntax error")]
    [InlineData(2, 8, "Syntax error")]
    [InlineData(2, 7, "syntax error")]
    public void Equals_DifferentValue_IsFalse(int line, int column, string message)
    {
        var first = new DaxSyntaxError(2, 7, "Syntax error");
        var second = new DaxSyntaxError(line, column, message);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_OtherType_IsFalse()
    {
        var error = new DaxSyntaxError(2, 7, "Syntax error");

        Assert.False(error.Equals("Syntax error"));
    }

    [Fact]
    public void Constructor_NegativePosition_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>("line", () => new DaxSyntaxError(-1, 0, "m"));
        Assert.Throws<ArgumentOutOfRangeException>("column", () => new DaxSyntaxError(0, -1, "m"));
    }

    [Fact]
    public void Constructor_NullMessage_Throws()
    {
        Assert.Throws<ArgumentNullException>("message", () => new DaxSyntaxError(0, 0, null!));
    }
}
