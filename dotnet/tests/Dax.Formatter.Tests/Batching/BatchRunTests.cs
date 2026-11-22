using System.Net;
using Dax.Formatter.Configuration;
using Dax.Formatter.Tests.Fakes;
using Xunit;

namespace Dax.Formatter.Tests.Batching;

public class BatchRunTests
{
    private static readonly BatchLimits s_oneTextPerRequest = new(maxTexts: 1, maxCharacters: 1000);
    private static readonly BatchLimits s_twoTextsPerRequest = new(maxTexts: 2, maxCharacters: 1000);
    private static readonly string[] s_fiveTexts = ["a", "b", "c", "d", "e"];

    [Fact]
    public async Task FormatBatchAsync_AnswersOutOfOrder_ReturnsTheResultsInTheOrderOfTheTexts()
    {
        // The first two requests are answered only after the third one has arrived.
        var responder = Respond.OnceArrived(3, Respond.Echo);
        using var harness = new FormatterHarness(
            responder, options => options.MaxConcurrentRequests = 3, s_twoTextsPerRequest);

        var results = await harness.Client.FormatBatchAsync(s_fiveTexts).ConfigureAwait(true);

        Assert.Equal(3, harness.Handler.Requests.Count);
        Assert.Equal(s_fiveTexts, results.Select(result => result.FormattedText));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task FormatBatchAsync_MoreRequestsThanAllowedAtOnce_KeepsToTheLimit(int maxConcurrentRequests)
    {
        // Nothing is answered until as many requests as the limit allows are in progress.
        var responder = Respond.OnceArrived(maxConcurrentRequests, Respond.Echo);
        using var harness = new FormatterHarness(
            responder, options => options.MaxConcurrentRequests = maxConcurrentRequests, s_oneTextPerRequest);

        await harness.Client.FormatBatchAsync(s_fiveTexts).ConfigureAwait(true);

        Assert.Equal(5, harness.Handler.Requests.Count);
        Assert.Equal(maxConcurrentRequests, harness.Handler.MaxInFlight);
    }

    [Fact]
    public async Task FormatBatchAsync_RequestsOfOneCall_ShareTheOperationIdOnly()
    {
        using var harness = new FormatterHarness(Respond.Echo, limits: s_twoTextsPerRequest);

        await harness.Client.FormatBatchAsync(s_fiveTexts).ConfigureAwait(true);

        var requests = harness.Handler.Requests;
        Assert.Single(requests.Select(request => request.GetHeader("DaxFormatter-Operation-Id")).Distinct());
        Assert.Equal(3, requests.Select(request => request.GetHeader("DaxFormatter-Request-Id")).Distinct().Count());
    }

    [Fact]
    public async Task FormatBatchAsync_OneRequestFails_StopsTheOthersAndThrowsItsFailure()
    {
        // Once the three requests are in progress, the one that starts with "c" fails and the others never answer.
        Responder failOne = (request, cancellationToken) =>
        {
            var answer = request.Dax[0] == "c" ? Respond.Status(HttpStatusCode.BadRequest) : Respond.Hang;
            return answer(request, cancellationToken);
        };
        var responder = Respond.OnceArrived(3, failOne);
        using var harness = new FormatterHarness(
            responder, options => options.MaxConcurrentRequests = 3, s_twoTextsPerRequest);

        var exception = await Assert
            .ThrowsAsync<DaxFormatterException>(() => harness.Client.FormatBatchAsync(s_fiveTexts))
            .ConfigureAwait(true);

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Contains("the texts at indexes 2 to 3", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FormatBatchAsync_FailedRequestAroundABlankText_ReportsTheTextsItCarriedAndTheCall()
    {
        string[] texts = ["a", " ", "c"];
        int[] sentIndexes = [0, 2];
        using var harness = new FormatterHarness(Respond.Status(HttpStatusCode.BadRequest), limits: s_twoTextsPerRequest);

        var exception = await Assert
            .ThrowsAsync<DaxFormatterException>(() => harness.Client.FormatBatchAsync(texts))
            .ConfigureAwait(true);

        Assert.Equal(sentIndexes, exception.TextIndexes);
        Assert.Equal(harness.Handler.Requests[0].GetHeader("DaxFormatter-Operation-Id"), exception.OperationId);
    }

    [Fact]
    public async Task FormatBatchAsync_OneRequestFails_DoesNotSendTheRemainingOnes()
    {
        var responder = Respond.Sequence(Respond.Echo, Respond.Status(HttpStatusCode.BadRequest), Respond.Echo);
        using var harness = new FormatterHarness(
            responder, options => options.MaxConcurrentRequests = 1, s_oneTextPerRequest);

        var exception = await Assert
            .ThrowsAsync<DaxFormatterException>(() => harness.Client.FormatBatchAsync(s_fiveTexts))
            .ConfigureAwait(true);

        Assert.Equal(2, harness.Handler.Requests.Count);
        Assert.Contains("the text at index 1 ", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FormatBatchAsync_CallerCancels_ThrowsOperationCanceledWithTheTokenOfTheCaller()
    {
        using var cancellation = new CancellationTokenSource();
        var responder = Respond.OnArrival(cancellation.Cancel, Respond.Hang);
        using var harness = new FormatterHarness(responder, limits: s_oneTextPerRequest);

        var exception = await Assert
            .ThrowsAnyAsync<OperationCanceledException>(
                () => harness.Client.FormatBatchAsync(s_fiveTexts, cancellationToken: cancellation.Token))
            .ConfigureAwait(true);

        Assert.Equal(cancellation.Token, exception.CancellationToken);
    }

    [Fact]
    public async Task FormatBatchAsync_CallerCancelsWhileARequestFails_ThrowsOperationCanceled()
    {
        using var cancellation = new CancellationTokenSource();
        var responder = Respond.OnArrival(cancellation.Cancel, Respond.Status(HttpStatusCode.BadRequest));
        using var harness = new FormatterHarness(responder, limits: s_oneTextPerRequest);

        var exception = await Assert
            .ThrowsAnyAsync<OperationCanceledException>(
                () => harness.Client.FormatBatchAsync(s_fiveTexts, cancellationToken: cancellation.Token))
            .ConfigureAwait(true);

        Assert.Equal(cancellation.Token, exception.CancellationToken);
    }

    [Fact]
    public async Task FormatBatchAsync_TokenAlreadyCancelled_SendsNothing()
    {
        var cancelled = new CancellationToken(canceled: true);
        using var harness = new FormatterHarness(Respond.Echo);

        await Assert
            .ThrowsAnyAsync<OperationCanceledException>(
                () => harness.Client.FormatBatchAsync(s_fiveTexts, cancellationToken: cancelled))
            .ConfigureAwait(true);

        Assert.Empty(harness.Handler.Requests);
    }

    [Fact]
    public async Task FormatBatchAsync_NoTextsWithTokenAlreadyCancelled_Throws()
    {
        var cancelled = new CancellationToken(canceled: true);
        using var harness = new FormatterHarness(Respond.Echo);

        await Assert
            .ThrowsAnyAsync<OperationCanceledException>(
                () => harness.Client.FormatBatchAsync([], cancellationToken: cancelled))
            .ConfigureAwait(true);
    }

    [Fact]
    public async Task FormatBatchAsync_ProgressReceiverCancelsOnItsOwn_ThrowsTheExceptionOfTheReceiver()
    {
        using var harness = new FormatterHarness(Respond.Echo, limits: s_oneTextPerRequest);
        var progress = new CancellingProgress();

        var exception = await Assert
            .ThrowsAnyAsync<OperationCanceledException>(
                () => harness.Client.FormatBatchAsync(s_fiveTexts, progress: progress))
            .ConfigureAwait(true);

        Assert.Equal(CancellingProgress.Message, exception.Message);
    }

    [Fact]
    public async Task FormatBatchAsync_ResultWithoutTheHeaderOfTheClient_FailsAndNamesTheText()
    {
        var withoutHeader = Respond.Json("[{\"formatted\":\"1 + 2\\r\\n\",\"errors\":[]}]");
        using var harness = new FormatterHarness(
            Respond.Sequence(Respond.Echo, withoutHeader),
            options => options.MaxConcurrentRequests = 1,
            s_oneTextPerRequest);
        var options = new DaxFormatOptions { Kind = DaxKind.Expression };

        var exception = await Assert
            .ThrowsAsync<DaxFormatterException>(() => harness.Client.FormatBatchAsync(["1", "1+2"], options))
            .ConfigureAwait(true);

        Assert.Equal(DaxFormatterFailureReason.InvalidResponse, exception.Reason);
        Assert.Contains("the text at index 1 ", exception.Message, StringComparison.Ordinal);
        Assert.Equal(Respond.ServiceRequestId, exception.ServiceRequestId);
    }

    [Fact]
    public async Task FormatBatchAsync_FailedRequest_KeepsTheStackTraceOfTheFailure()
    {
        using var harness = new FormatterHarness(Respond.Status(HttpStatusCode.BadRequest), limits: s_oneTextPerRequest);

        var exception = await Assert
            .ThrowsAsync<DaxFormatterException>(() => harness.Client.FormatBatchAsync(s_fiveTexts))
            .ConfigureAwait(true);

        Assert.Contains(nameof(Dax.Formatter.Transport.RetryingSender), exception.StackTrace, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FormatBatchAsync_BlankTexts_CountsThemBeforeTheFirstRequest()
    {
        var progress = new ListProgress();
        using var harness = new FormatterHarness(
            Respond.Echo, options => options.MaxConcurrentRequests = 1, s_oneTextPerRequest);

        await harness.Client.FormatBatchAsync(["", "a", "b", " "], progress: progress).ConfigureAwait(true);

        Assert.Equal([(2, 4), (3, 4), (4, 4)], progress.Reports);
    }

    // A receiver that cancels with a token of its own, which the client knows nothing about.
    private sealed class CancellingProgress : IProgress<DaxFormatProgressInfo>
    {
        public const string Message = "The receiver of the progress cancelled.";

        public void Report(DaxFormatProgressInfo value)
        {
            throw new OperationCanceledException(Message);
        }
    }
}
