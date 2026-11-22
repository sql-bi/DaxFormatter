using Xunit;

namespace Dax.Formatter.Tests.Live;

/// <summary>
/// Puts the tests that call the live service in one collection, so that they run one at a time.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class LiveServiceTestGroup
{
    public const string Name = "Live service";
}
