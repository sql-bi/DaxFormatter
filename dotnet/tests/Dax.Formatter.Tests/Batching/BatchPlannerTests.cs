using Dax.Formatter.Batching;
using Dax.Formatter.Configuration;
using Dax.Formatter.Expressions;
using Dax.Formatter.Tests.Fixtures;
using Xunit;

namespace Dax.Formatter.Tests.Batching;

public class BatchPlannerTests
{
    private const string Fixture = "format-results.json";

    public static IEnumerable<object[]> FixtureCases => FixtureFile.CaseNames(Fixture);

    [Theory]
    [InlineData(5, 2, "0,1|2,3|4")]
    [InlineData(4, 2, "0,1|2,3")]
    [InlineData(3, 5, "0,1,2")]
    [InlineData(2, 1, "0|1")]
    [InlineData(0, 2, "")]
    public void Plan_MoreTextsThanARequestCarries_SplitsThemByCount(int textCount, int maxTexts, string expected)
    {
        var texts = Enumerable.Repeat("EVALUATE Sales", textCount).ToArray();

        var plan = BatchPlanner.Plan(texts, DaxKind.Auto, new BatchLimits(maxTexts, maxCharacters: 1000));

        Assert.Equal(expected, Describe(plan));
    }

    [Theory]
    [InlineData(new[] { 4, 4, 4 }, 8, "0,1|2")]
    [InlineData(new[] { 4, 4, 4 }, 7, "0|1|2")]
    [InlineData(new[] { 3, 20, 3 }, 10, "0|1|2")]
    [InlineData(new[] { 20, 3, 3 }, 10, "0|1,2")]
    [InlineData(new[] { 3, 3, 20 }, 10, "0,1|2")]
    public void Plan_MoreCharactersThanARequestCarries_SplitsThemByLength(
        int[] lengths, int maxCharacters, string expected)
    {
        var texts = lengths.Select(length => new string('x', length)).ToArray();

        var plan = BatchPlanner.Plan(texts, DaxKind.Auto, new BatchLimits(maxTexts: 100, maxCharacters));

        Assert.Equal(expected, Describe(plan));
    }

    [Fact]
    public void Plan_BlankTexts_LeavesThemOutOfTheRequests()
    {
        string[] texts = ["a", "", " \r\n\t", "b", "c"];

        var plan = BatchPlanner.Plan(texts, DaxKind.Auto, new BatchLimits(maxTexts: 2, maxCharacters: 1000));

        Assert.Equal([1, 2], plan.BlankIndexes);
        Assert.Equal("0,3|4", Describe(plan));
    }

    [Fact]
    public void Plan_Expressions_ChoosesTheHeaderOfEachText()
    {
        string[] texts = ["SUM(Sales[Amount])", "(a) => a", ""];

        var plan = BatchPlanner.Plan(texts, DaxKind.Expression, BatchLimits.Default);

        Assert.Same(ExpressionHeader.Measure, plan.GetHeader(0));
        Assert.Same(ExpressionHeader.Function, plan.GetHeader(1));
        Assert.Null(plan.GetHeader(2));
    }

    [Fact]
    public void Plan_TextsSentAsTheyAre_HaveNoHeader()
    {
        string[] texts = ["(a) => a"];

        var plan = BatchPlanner.Plan(texts, DaxKind.Auto, BatchLimits.Default);

        Assert.Null(plan.GetHeader(0));
    }

    [Theory]
    [MemberData(nameof(FixtureCases))]
    public void Plan_FixtureText_SendsItAsTheServiceReceivedIt(string name)
    {
        var testCase = FixtureFile.Case(Fixture, name);
        string[] texts = [testCase.GetProperty("text").GetString()!];

        var plan = BatchPlanner.Plan(texts, FixtureOptions.ReadKind(testCase), BatchLimits.Default);

        var chunk = Assert.Single(plan.Chunks);
        var sent = Assert.Single(plan.GetTextsToSend(chunk));
        Assert.Equal(testCase.GetProperty("sent").GetString(), sent);
    }

    [Fact]
    public void Plan_Chunk_KnowsItsFirstAndLastText()
    {
        string[] texts = ["a", "", "b", "c"];

        var plan = BatchPlanner.Plan(texts, DaxKind.Auto, new BatchLimits(maxTexts: 2, maxCharacters: 1000));

        Assert.Equal((0, 2), (plan.Chunks[0].FirstIndex, plan.Chunks[0].LastIndex));
        Assert.Equal((3, 3), (plan.Chunks[1].FirstIndex, plan.Chunks[1].LastIndex));
    }

    private static string Describe(BatchPlan plan)
    {
        var chunks = plan.Chunks.Select(
            chunk => string.Join(",", Enumerable.Range(0, chunk.Count).Select(position => chunk[position])));
        return string.Join("|", chunks);
    }
}
