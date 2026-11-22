using Dax.Formatter.Tests.Fakes;
using Xunit;

namespace Dax.Formatter.Tests;

/// <summary>
/// Calls the client the way applications do. The declared types of the results pin which overload each call
/// binds to: a change that moves a call to another overload stops this file from compiling.
/// </summary>
public class PublicApiUsageTests
{
    private static readonly string[] s_texts = ["EVALUATE Sales", "EVALUATE Customer"];

    [Fact]
    public async Task FormatBatchAsync_StringArray_BindsToTheOverloadForTexts()
    {
        using var harness = new FormatterHarness(Respond.Echo);
        string[] texts = s_texts;

        IReadOnlyList<DaxFormatResult> results = await harness.Client.FormatBatchAsync(texts).ConfigureAwait(true);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task FormatBatchAsync_ListOfStrings_BindsToTheOverloadForTexts()
    {
        using var harness = new FormatterHarness(Respond.Echo);
        List<string> texts = [.. s_texts];
        var options = new DaxFormatOptions { Kind = DaxKind.Expression };

        IReadOnlyList<DaxFormatResult> results = await harness.Client
            .FormatBatchAsync(texts, options, cancellationToken: CancellationToken.None)
            .ConfigureAwait(true);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task FormatBatchAsync_TextsWithNamedOptionalArguments_BindsToTheOverloadForTexts()
    {
        using var harness = new FormatterHarness(Respond.Echo);
        var progress = new ListProgress();

        IReadOnlyList<DaxFormatResult> results = await harness.Client
            .FormatBatchAsync(s_texts, options: null, progress: progress)
            .ConfigureAwait(true);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task FormatBatchAsync_ItemsAndSelector_BindsToTheOverloadForItems()
    {
        using var harness = new FormatterHarness(Respond.Echo);
        List<Uri> items = [new Uri("https://example.test/a"), new Uri("https://example.test/b")];

        IReadOnlyList<DaxFormatResult<Uri>> results = await harness.Client
            .FormatBatchAsync(items, item => "EVALUATE " + item.Segments[1])
            .ConfigureAwait(true);

        Assert.Equal(items, results.Select(result => result.Item));
    }

    [Fact]
    public async Task FormatBatchAsync_StringsAndSelector_BindsToTheOverloadForItems()
    {
        using var harness = new FormatterHarness(Respond.Echo);
        string[] tableNames = ["Sales", "Customer"];
        var options = new DaxFormatOptions { LineStyle = DaxLineStyle.ShortLine };

        IReadOnlyList<DaxFormatResult<string>> results = await harness.Client
            .FormatBatchAsync(tableNames, tableName => "EVALUATE " + tableName, options)
            .ConfigureAwait(true);

        Assert.Equal(tableNames, results.Select(result => result.Item));
    }

    [Fact]
    public async Task FormatAsync_ResultThatSucceeded_HasAFormattedTextThatIsNotNull()
    {
        using var harness = new FormatterHarness(Respond.Echo);

        DaxFormatResult result = await harness.Client.FormatAsync("EVALUATE Sales").ConfigureAwait(true);

        // No null check and no "!" below: the compiler knows that the text is there when Succeeded is true.
        var length = result.Succeeded ? result.FormattedText.Length : -1;
        Assert.Equal(14, length);
    }
}
