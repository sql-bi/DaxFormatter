using Xunit;

namespace Dax.Formatter.Tests.Live;

/// <summary>
/// Marks a test that calls the live DAX Formatter service. The test is skipped unless the environment variable
/// <see cref="EnvironmentVariable"/> is set, so that a normal run needs no network.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class LiveFactAttribute : FactAttribute
{
    public const string EnvironmentVariable = "DAXFORMATTER_LIVE_TESTS";

    public LiveFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EnvironmentVariable)))
        {
            Skip = "Calls the live service: set " + EnvironmentVariable + " to run it.";
        }
    }
}
