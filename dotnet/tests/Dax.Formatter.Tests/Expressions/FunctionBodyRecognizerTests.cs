using Dax.Formatter.Expressions;
using Dax.Formatter.Tests.Fixtures;
using Xunit;

namespace Dax.Formatter.Tests.Expressions;

public class FunctionBodyRecognizerTests
{
    private const string Fixture = "function-bodies.json";

    public static IEnumerable<object[]> Cases => FixtureFile.CaseNames(Fixture);

    [Theory]
    [MemberData(nameof(Cases))]
    public void IsFunctionBody_FixtureText_ReturnsTheExpectedAnswer(string name)
    {
        var testCase = FixtureFile.Case(Fixture, name);
        var text = testCase.GetProperty("text").GetString()!;

        var isFunction = FunctionBodyRecognizer.IsFunctionBody(text);

        Assert.Equal(testCase.GetProperty("isFunction").GetBoolean(), isFunction);
    }
}
