using Dax.Formatter.Expressions;
using Xunit;

namespace Dax.Formatter.Tests.Expressions;

public class DaxTextScannerTests
{
    [Theory]
    [InlineData("\"a\"", "String:\"a\"")]
    [InlineData("\"a\"\"b\" ", "String:\"a\"\"b\"|Other: ")]
    [InlineData("\"a\nb\"", "String:\"a\nb\"")]
    [InlineData("'My ''T'[c]", "QuotedName:'My ''T'|BracketName:[c]")]
    [InlineData("[a]]b]+", "BracketName:[a]]b]|Other:+")]
    [InlineData("// c\nx", "LineComment:// c|LineBreak:\n|Other:x")]
    [InlineData("-- c\r\nx", "LineComment:-- c\r|LineBreak:\n|Other:x")]
    [InlineData("/* a\n b */x", "BlockComment:/* a\n b */|Other:x")]
    [InlineData("1-2/3", "Other:1|Other:-|Other:2|Other:/|Other:3")]
    [InlineData("\"//\" /* ' */ '--'", "String:\"//\"|Other: |BlockComment:/* ' */|Other: |QuotedName:'--'")]
    public void Read_Text_ReturnsItsPieces(string text, string expected)
    {
        var pieces = ReadAll(text);

        Assert.Equal(expected, string.Join("|", pieces));
    }

    [Theory]
    [InlineData("\"abc", "String")]
    [InlineData("'abc", "QuotedName")]
    [InlineData("[abc", "BracketName")]
    [InlineData("/* abc", "BlockComment")]
    [InlineData("// abc", "LineComment")]
    public void Read_PieceNotClosed_LastsUntilTheEnd(string text, string expectedKind)
    {
        var pieces = ReadAll(text);

        Assert.Equal(expectedKind + ":" + text, Assert.Single(pieces));
    }

    [Fact]
    public void Read_EmptyText_ReturnsEnd()
    {
        var scanner = new DaxTextScanner(string.Empty, 0);

        Assert.Equal(DaxTokenKind.End, scanner.Read());
    }

    private static List<string> ReadAll(string text)
    {
        var scanner = new DaxTextScanner(text, 0);
        var pieces = new List<string>();
        while (true)
        {
            var kind = scanner.Read();
            if (kind == DaxTokenKind.End)
            {
                return pieces;
            }

            pieces.Add(kind + ":" + text.AsSpan(scanner.TokenStart, scanner.Position - scanner.TokenStart).ToString());
        }
    }
}
