using System.Text.Json;

namespace Dax.Formatter.Tests.Fixtures;

/// <summary>
/// Reads the JSON files of this folder, which are embedded in the test assembly. They hold the cases of the
/// wire format in a form that the clients of other languages can share.
/// </summary>
internal static class FixtureFile
{
    public static JsonElement Load(string fileName)
    {
        using var stream = OpenResource("Fixtures/" + fileName);
        using var document = JsonDocument.Parse(stream);
        return document.RootElement.Clone();
    }

    /// <summary>Gets the names of the cases of a file, which a theory takes as its data.</summary>
    public static IEnumerable<object[]> CaseNames(string fileName)
    {
        return Cases(fileName).Select(testCase => new object[] { testCase.GetProperty("name").GetString()! });
    }

    public static JsonElement Case(string fileName, string name)
    {
        return Cases(fileName).Single(testCase => testCase.GetProperty("name").GetString() == name);
    }

    public static Stream OpenResource(string name)
    {
        var stream = typeof(FixtureFile).Assembly.GetManifestResourceStream(name);
        return stream ?? throw new InvalidOperationException($"The embedded resource '{name}' is missing.");
    }

    /// <summary>Reads an embedded text file with line feeds as its line breaks, whatever the checkout wrote.</summary>
    public static string ReadText(string name)
    {
        using var stream = OpenResource(name);
        using var reader = new StreamReader(stream);

        var lines = new List<string>();
        while (reader.ReadLine() is string line)
        {
            lines.Add(line);
        }

        return string.Join("\n", lines);
    }

    private static JsonElement.ArrayEnumerator Cases(string fileName)
    {
        return Load(fileName).GetProperty("cases").EnumerateArray();
    }
}
