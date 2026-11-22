using Dax.Formatter.Tests.Fixtures;
using Dax.Formatter.Wire;
using Xunit;

namespace Dax.Formatter.Tests.Wire;

public class NameHashTests
{
    private const string Fixture = "name-hashes.json";

    public static IEnumerable<object[]> Cases => FixtureFile.CaseNames(Fixture);

    [Theory]
    [MemberData(nameof(Cases))]
    public void Compute_FixtureName_ReturnsItsHash(string name)
    {
        var expected = FixtureFile.Case(Fixture, name).GetProperty("hash").GetString();

        var hash = NameHash.Compute(name);

        Assert.Equal(expected, hash);
    }
}
