using System.Text.RegularExpressions;
using Dax.Formatter.Tests.Fixtures;
using Xunit;

namespace Dax.Formatter.Tests.Readme;

/// <summary>
/// Keeps the C# code of the package readme equal to code that compiles and runs: the regions of
/// <see cref="ReadmeSamples"/>. A readme on nuget.org can be corrected only with a new version of the package.
/// </summary>
public class ReadmeTests
{
    private static readonly Regex s_codeBlock = new(@"^```csharp\n(?<code>.*?)\n```$", RegexOptions.Multiline | RegexOptions.Singleline);
    private static readonly Regex s_region = new(@"^[ ]*#region (?<name>\w+)\n(?<code>.*?)\n[ ]*#endregion", RegexOptions.Multiline | RegexOptions.Singleline);

    public static IEnumerable<object[]> RegionNames => ReadRegions().Keys.Select(name => new object[] { name });

    [Fact]
    public void Readme_EveryCSharpBlock_IsARegionOfTheSamples()
    {
        var regions = ReadRegions().Values.ToList();

        var blocks = ReadCodeBlocks();

        Assert.NotEmpty(blocks);
        Assert.All(blocks, block => Assert.Contains(block, regions));
    }

    [Theory]
    [MemberData(nameof(RegionNames))]
    public void Samples_EveryRegion_IsACSharpBlockOfTheReadme(string name)
    {
        var region = ReadRegions()[name];

        var blocks = ReadCodeBlocks();

        Assert.Contains(region, blocks);
    }

    private static List<string> ReadCodeBlocks()
    {
        var readme = FixtureFile.ReadText("README.md");
        return [.. s_codeBlock.Matches(readme).Cast<Match>().Select(match => match.Groups["code"].Value)];
    }

    private static Dictionary<string, string> ReadRegions()
    {
        var source = FixtureFile.ReadText("ReadmeSamples.cs");
        return s_region.Matches(source).Cast<Match>().ToDictionary(
            match => match.Groups["name"].Value,
            match => RemoveIndentation(match.Groups["code"].Value));
    }

    // A region sits inside a method: its lines lose the indentation of the least indented one.
    private static string RemoveIndentation(string code)
    {
        var lines = code.Split('\n');
        var indentation = lines.Where(line => line.Trim().Length > 0).Min(line => line.Length - line.TrimStart().Length);
        return string.Join("\n", lines.Select(line => line.Length >= indentation ? line.Substring(indentation) : line.Trim()));
    }
}
