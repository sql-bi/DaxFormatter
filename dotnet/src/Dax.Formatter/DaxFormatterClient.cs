using Dax.Formatter.Batching;
using Dax.Formatter.Configuration;
using Dax.Formatter.Transport;

namespace Dax.Formatter;

/// <summary>
/// Formats DAX text with the DAX Formatter service. An instance is thread-safe and meant to be reused: create
/// one and keep it for the life of the application.
/// </summary>
/// <remarks>
/// A syntax error in a text is part of its <see cref="DaxFormatResult"/>. A failure of the service or of the
/// network throws <see cref="DaxFormatterException"/>, and a cancellation by the caller throws
/// <see cref="OperationCanceledException"/>.
/// </remarks>
public class DaxFormatterClient
{
    private readonly Lazy<BatchFormatter> _formatter;

    /// <summary>
    /// Initializes a new instance of the <see cref="DaxFormatterClient"/> class with the default options. It
    /// uses an <see cref="HttpClient"/> that all such instances share.
    /// </summary>
    public DaxFormatterClient()
        : this(new DaxFormatterClientOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DaxFormatterClient"/> class with the given options. It uses
    /// an <see cref="HttpClient"/> that all such instances share.
    /// </summary>
    /// <param name="options">The options, which are copied.</param>
    public DaxFormatterClient(DaxFormatterClientOptions options)
        : this(httpClient: null, ClientSettings.From(options), TaskDelay.Instance, BatchLimits.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DaxFormatterClient"/> class that sends its requests with
    /// the given <see cref="HttpClient"/>.
    /// </summary>
    /// <remarks>
    /// This is the only constructor that takes an <see cref="HttpClient"/>, so that
    /// <c>services.AddHttpClient&lt;DaxFormatterClient&gt;()</c> can create the client: the container passes
    /// the options when it has them.
    /// </remarks>
    /// <param name="httpClient">
    /// The <see cref="HttpClient"/> to use. The caller owns it: the client never changes or disposes it, and
    /// ignores its <see cref="HttpClient.BaseAddress"/>.
    /// </param>
    /// <param name="options">The options, which are copied, or <see langword="null"/> for the defaults.</param>
    public DaxFormatterClient(HttpClient httpClient, DaxFormatterClientOptions? options = null)
        : this(httpClient, options ?? new DaxFormatterClientOptions(), TaskDelay.Instance, BatchLimits.Default)
    {
    }

    internal DaxFormatterClient(
        HttpClient httpClient,
        DaxFormatterClientOptions options,
        IDelay delay,
        BatchLimits limits)
        : this(Required(httpClient), ClientSettings.From(options), delay, limits)
    {
    }

    // Nothing is created or resolved here: a class that derives from this one, such as a mock, pays nothing.
    // A creation that fails is not kept: the next call tries again. Two first calls at the same time can each
    // create a formatter, which holds no resources, and one of them is kept.
    private DaxFormatterClient(HttpClient? httpClient, ClientSettings settings, IDelay delay, BatchLimits limits)
    {
        _formatter = new Lazy<BatchFormatter>(
            () => BatchFormatter.Create(httpClient ?? SharedHttpClient.Instance, settings, delay, limits),
            LazyThreadSafetyMode.PublicationOnly);
    }

    /// <summary>
    /// Formats one DAX text.
    /// </summary>
    /// <param name="text">The DAX text. A blank text is formatted to an empty text without a request.</param>
    /// <param name="options">The options of the call, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">The token that cancels the call.</param>
    /// <returns>The formatted text, or the syntax errors of the text.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="options"/> has a value that is not valid.</exception>
    /// <exception cref="DaxFormatterException">The service or the network failed.</exception>
    /// <exception cref="OperationCanceledException">The call was cancelled.</exception>
    public virtual Task<DaxFormatResult> FormatAsync(
        string text,
        DaxFormatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);
        var settings = FormatSettings.From(options);

        return FormatOneAsync(text, settings, cancellationToken);
    }

    /// <summary>
    /// Formats several DAX texts with as few requests as they need, and returns one result for each text, in
    /// the order of the texts.
    /// </summary>
    /// <param name="texts">The DAX texts. They are read once, before the first request.</param>
    /// <param name="options">The options of the call, or <see langword="null"/> for the defaults.</param>
    /// <param name="progress">The receiver of the number of texts done, or <see langword="null"/>.</param>
    /// <param name="cancellationToken">The token that cancels the call.</param>
    /// <returns>One result for each text, or an empty list for no texts.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="texts"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// A text is <see langword="null"/>, or <paramref name="options"/> has a value that is not valid.
    /// </exception>
    /// <exception cref="DaxFormatterException">The service or the network failed: no result is returned.</exception>
    /// <exception cref="OperationCanceledException">The call was cancelled.</exception>
    public virtual Task<IReadOnlyList<DaxFormatResult>> FormatBatchAsync(
        IEnumerable<string> texts,
        DaxFormatOptions? options = null,
        IProgress<DaxFormatProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(texts);

        string[] copy = [.. texts];
        var missing = Array.IndexOf(copy, null);
        if (missing >= 0)
        {
            var message = FormattableString.Invariant($"The text at index {missing} is null.");
            throw new ArgumentException(message, nameof(texts));
        }

        var settings = FormatSettings.From(options);

        return FormatManyAsync(copy, settings, progress, cancellationToken);
    }

    /// <summary>
    /// Formats the DAX text of several items, such as the measures of a model, and returns one result for each
    /// item, in the order of the items, each with the item it belongs to.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="items">The items. They are read once, before the first request.</param>
    /// <param name="textSelector">The function that returns the DAX text of an item.</param>
    /// <param name="options">The options of the call, or <see langword="null"/> for the defaults.</param>
    /// <param name="progress">The receiver of the number of texts done, or <see langword="null"/>.</param>
    /// <param name="cancellationToken">The token that cancels the call.</param>
    /// <returns>One result for each item, or an empty list for no items.</returns>
    /// <remarks>
    /// This method calls
    /// <see cref="FormatBatchAsync(IEnumerable{string}, DaxFormatOptions?, IProgress{DaxFormatProgressInfo}?, CancellationToken)"/>,
    /// so a class that overrides that method changes this one too.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> or <paramref name="textSelector"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="textSelector"/> returned <see langword="null"/>, or <paramref name="options"/> has a value
    /// that is not valid.
    /// </exception>
    /// <exception cref="DaxFormatterException">The service or the network failed: no result is returned.</exception>
    /// <exception cref="OperationCanceledException">The call was cancelled.</exception>
    public Task<IReadOnlyList<DaxFormatResult<TItem>>> FormatBatchAsync<TItem>(
        IEnumerable<TItem> items,
        Func<TItem, string> textSelector,
        DaxFormatOptions? options = null,
        IProgress<DaxFormatProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(textSelector);

        TItem[] copy = [.. items];
        var texts = new string[copy.Length];
        for (var index = 0; index < copy.Length; index++)
        {
            var text = textSelector(copy[index]);
            if (text is null)
            {
                var message = FormattableString.Invariant(
                    $"The text selector returned null for the item at index {index}.");
                throw new ArgumentException(message, nameof(textSelector));
            }

            texts[index] = text;
        }

        // The options are checked here so that a wrong value throws now, like the other arguments.
        _ = FormatSettings.From(options);

        return FormatItemsAsync(copy, texts, options, progress, cancellationToken);
    }

    private static HttpClient Required(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        return httpClient;
    }

    private async Task<DaxFormatResult> FormatOneAsync(
        string text,
        FormatSettings settings,
        CancellationToken cancellationToken)
    {
        var results = await _formatter.Value
            .FormatAsync([text], settings, progress: null, cancellationToken)
            .ConfigureAwait(false);

        return results[0];
    }

    private async Task<IReadOnlyList<DaxFormatResult>> FormatManyAsync(
        string[] texts,
        FormatSettings settings,
        IProgress<DaxFormatProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (texts.Length == 0)
        {
            return [];
        }

        return await _formatter.Value
            .FormatAsync(texts, settings, progress, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<DaxFormatResult<TItem>>> FormatItemsAsync<TItem>(
        TItem[] items,
        string[] texts,
        DaxFormatOptions? options,
        IProgress<DaxFormatProgressInfo>? progress,
        CancellationToken cancellationToken)
    {
        var results = await FormatBatchAsync(texts, options, progress, cancellationToken).ConfigureAwait(false);
        if (results is null || results.Count != items.Length)
        {
            throw new InvalidOperationException("FormatBatchAsync must return one result for each text.");
        }

        var paired = new DaxFormatResult<TItem>[items.Length];
        for (var index = 0; index < items.Length; index++)
        {
            paired[index] = new DaxFormatResult<TItem>(items[index], results[index]);
        }

        return paired;
    }
}
