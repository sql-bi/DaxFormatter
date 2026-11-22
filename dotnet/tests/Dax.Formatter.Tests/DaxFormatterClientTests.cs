using System.Collections;
using System.Net.Http;
using Dax.Formatter.Tests.Fakes;
using Xunit;

namespace Dax.Formatter.Tests;

public class DaxFormatterClientTests
{
    private static readonly DaxFormatOptions s_expression = new() { Kind = DaxKind.Expression };
    private static readonly DaxFormatOptions s_invalidOptions = new() { LineStyle = (DaxLineStyle)7 };

    [Fact]
    public void Constructor_NullArgument_Throws()
    {
        using var httpClient = new HttpClient();

        Assert.Throws<ArgumentNullException>("httpClient", () => new DaxFormatterClient((HttpClient)null!));
        Assert.Throws<ArgumentNullException>("options", () => new DaxFormatterClient((DaxFormatterClientOptions)null!));
        Assert.Throws<ArgumentNullException>("options", () => new DaxFormatterClient(httpClient, null!));
    }

    [Fact]
    public void Constructor_OptionOutOfRange_Throws()
    {
        var options = new DaxFormatterClientOptions { MaxConcurrentRequests = 0 };

        Assert.Throws<ArgumentException>("options", () => new DaxFormatterClient(options));
    }

    [Fact]
    public void FormatAsync_NullText_ThrowsBeforeReturningATask()
    {
        using var harness = new FormatterHarness(Respond.Echo);

        Assert.Throws<ArgumentNullException>("text", () => { _ = harness.Client.FormatAsync(null!); });
    }

    [Fact]
    public void FormatAsync_OptionOutsideItsEnumeration_ThrowsBeforeReturningATask()
    {
        using var harness = new FormatterHarness(Respond.Echo);

        Assert.Throws<ArgumentException>("options", () => { _ = harness.Client.FormatAsync("1", s_invalidOptions); });
    }

    [Fact]
    public void FormatBatchAsync_NullTexts_ThrowsBeforeReturningATask()
    {
        using var harness = new FormatterHarness(Respond.Echo);

        Assert.Throws<ArgumentNullException>(
            "texts", () => { _ = harness.Client.FormatBatchAsync((IEnumerable<string>)null!); });
    }

    [Fact]
    public void FormatBatchAsync_NullText_ThrowsWithItsIndex()
    {
        using var harness = new FormatterHarness(Respond.Echo);

        var exception = Assert.Throws<ArgumentException>(
            "texts", () => { _ = harness.Client.FormatBatchAsync(["a", "b", null!, null!]); });

        Assert.Contains("index 2", exception.Message, StringComparison.Ordinal);
        Assert.Empty(harness.Handler.Requests);
    }

    [Fact]
    public void FormatBatchAsync_NullItemsOrSelector_ThrowsBeforeReturningATask()
    {
        using var harness = new FormatterHarness(Respond.Echo);
        int[] items = [1, 2];

        Assert.Throws<ArgumentNullException>(
            "items", () => { _ = harness.Client.FormatBatchAsync((IEnumerable<int>)null!, item => "1"); });
        Assert.Throws<ArgumentNullException>(
            "textSelector", () => { _ = harness.Client.FormatBatchAsync(items, (Func<int, string>)null!); });
    }

    [Fact]
    public void FormatBatchAsync_SelectorReturnsNull_ThrowsWithTheIndexOfTheItem()
    {
        using var harness = new FormatterHarness(Respond.Echo);
        int[] items = [1, 2, 3];

        var exception = Assert.Throws<ArgumentException>(
            "textSelector", () => { _ = harness.Client.FormatBatchAsync(items, item => item == 2 ? null! : "1"); });

        Assert.Contains("index 1", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void FormatBatchAsync_ItemsWithOptionOutsideItsEnumeration_ThrowsBeforeReturningATask()
    {
        using var harness = new FormatterHarness(Respond.Echo);
        int[] items = [1];

        Assert.Throws<ArgumentException>(
            "options", () => { _ = harness.Client.FormatBatchAsync(items, item => "1", s_invalidOptions); });
    }

    [Fact]
    public async Task FormatAsync_Query_ReturnsItFormattedWithoutTheFinalLineBreak()
    {
        using var harness = new FormatterHarness(Respond.Echo);

        var result = await harness.Client.FormatAsync("EVALUATE\nSales").ConfigureAwait(true);

        Assert.True(result.Succeeded);
        Assert.Equal(("EVALUATE\nSales", "EVALUATE\r\nSales"), (result.OriginalText, result.FormattedText));
    }

    [Fact]
    public async Task FormatAsync_Expression_ReturnsItWithoutTheHeader()
    {
        using var harness = new FormatterHarness(Respond.Echo);

        var result = await harness.Client.FormatAsync("SUM ( Sales[Amount] )", s_expression).ConfigureAwait(true);

        Assert.Equal("[x] :=\nSUM ( Sales[Amount] )", Assert.Single(harness.Handler.Requests[0].Dax));
        Assert.Equal("SUM ( Sales[Amount] )", result.FormattedText);
    }

    [Theory]
    [InlineData("(a) =>\n    a + 1", "(a) =>\r\n    a + 1")]
    [InlineData("// c\n(a) =>\n    a + 1", "// c\r\n(a) =>\r\n    a + 1")]
    public async Task FormatAsync_FunctionBody_ReturnsItWithoutTheHeaderAndItsIndentation(string text, string expected)
    {
        using var harness = new FormatterHarness(Respond.Echo);

        var result = await harness.Client.FormatAsync(text, s_expression).ConfigureAwait(true);

        Assert.StartsWith("FUNCTION x =\n", harness.Handler.Requests[0].Dax[0], StringComparison.Ordinal);
        Assert.Equal(expected, result.FormattedText);
    }

    [Fact]
    public async Task FormatAsync_SyntaxError_ReturnsItAsAResultWithItsPositionInTheText()
    {
        using var harness = new FormatterHarness(Respond.Echo);

        var result = await harness.Client.FormatAsync("1 +\n\t2 +* 3", s_expression).ConfigureAwait(true);

        Assert.False(result.Succeeded);
        Assert.Null(result.FormattedText);
        Assert.Equal(new DaxSyntaxError(1, 3, EchoService.ErrorMessage), Assert.Single(result.Errors));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \r\n\t")]
    public async Task FormatAsync_BlankText_ReturnsAnEmptyTextWithoutARequest(string text)
    {
        using var harness = new FormatterHarness(Respond.Echo);

        var result = await harness.Client.FormatAsync(text, s_expression).ConfigureAwait(true);

        Assert.Equal((text, string.Empty), (result.OriginalText, result.FormattedText));
        Assert.Empty(harness.Handler.Requests);
    }

    [Fact]
    public async Task FormatBatchAsync_NoTexts_ReturnsNoResultsWithoutARequest()
    {
        using var harness = new FormatterHarness(Respond.Echo);
        var progress = new ListProgress();

        var results = await harness.Client.FormatBatchAsync([], progress: progress).ConfigureAwait(true);

        Assert.Empty(results);
        Assert.Empty(harness.Handler.Requests);
        Assert.Empty(progress.Reports);
    }

    [Fact]
    public async Task FormatBatchAsync_BlankTextsAmongOthers_KeepsEachResultAtTheIndexOfItsText()
    {
        using var harness = new FormatterHarness(Respond.Echo);

        var results = await harness.Client.FormatBatchAsync(["", "1", " ", "2 +* 2"], s_expression).ConfigureAwait(true);

        Assert.Equal(["", "1", "", null], results.Select(result => result.FormattedText));
        Assert.Equal(["", "1", " ", "2 +* 2"], results.Select(result => result.OriginalText));
        Assert.Equal(2, Assert.Single(harness.Handler.Requests).Dax.Count);
    }

    [Fact]
    public async Task FormatBatchAsync_Items_ReturnsEachResultWithItsItem()
    {
        using var harness = new FormatterHarness(Respond.Echo);
        var measures = new[]
        {
            new KeyValuePair<string, string>("Sales", "SUM ( Sales[Amount] )"),
            new KeyValuePair<string, string>("Broken", "1 +* 2"),
            new KeyValuePair<string, string>("Cost", "SUM ( Sales[Cost] )"),
        };

        var results = await harness.Client
            .FormatBatchAsync(measures, measure => measure.Value, s_expression)
            .ConfigureAwait(true);

        Assert.Equal(measures, results.Select(result => result.Item));
        Assert.Equal(["SUM ( Sales[Amount] )", null, "SUM ( Sales[Cost] )"], results.Select(result => result.FormattedText));
    }

    [Fact]
    public async Task FormatBatchAsync_TextsThatCanBeReadOnce_ReadsThemOnce()
    {
        using var harness = new FormatterHarness(Respond.Echo);
        var texts = new CountingEnumerable(["EVALUATE A", "EVALUATE B"]);

        var results = await harness.Client.FormatBatchAsync(texts).ConfigureAwait(true);

        Assert.Equal(1, texts.Enumerations);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task FormatBatchAsync_ItemsOnADerivedClient_UsesItsOverrideForTexts()
    {
        var client = new CannedClient(texts => [.. texts.Select(text => DaxFormatResult.Success(text, "canned " + text))]);

        var results = await client.FormatBatchAsync([1, 2], item => "text " + item).ConfigureAwait(true);

        Assert.Equal([1, 2], results.Select(result => result.Item));
        Assert.Equal(["canned text 1", "canned text 2"], results.Select(result => result.FormattedText));
    }

    [Fact]
    public async Task FormatBatchAsync_OverrideReturnsTheWrongNumberOfResults_Throws()
    {
        var client = new CannedClient(_ => []);

        await Assert
            .ThrowsAsync<InvalidOperationException>(() => client.FormatBatchAsync([1, 2], item => "text"))
            .ConfigureAwait(true);
    }

    // What an application does to replace the client in its tests: nothing but the override runs.
    private sealed class CannedClient : DaxFormatterClient
    {
        private readonly Func<IEnumerable<string>, IReadOnlyList<DaxFormatResult>> _format;

        public CannedClient(Func<IEnumerable<string>, IReadOnlyList<DaxFormatResult>> format)
        {
            _format = format;
        }

        public override Task<IReadOnlyList<DaxFormatResult>> FormatBatchAsync(
            IEnumerable<string> texts,
            DaxFormatOptions? options = null,
            IProgress<DaxFormatProgressInfo>? progress = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_format(texts));
        }
    }

    private sealed class CountingEnumerable : IEnumerable<string>
    {
        private readonly string[] _texts;

        public CountingEnumerable(string[] texts)
        {
            _texts = texts;
        }

        public int Enumerations { get; private set; }

        public IEnumerator<string> GetEnumerator()
        {
            Enumerations++;
            return ((IEnumerable<string>)_texts).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
