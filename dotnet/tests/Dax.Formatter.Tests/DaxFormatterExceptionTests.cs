using System.Net;
using Xunit;

namespace Dax.Formatter.Tests;

public class DaxFormatterExceptionTests
{
    [Fact]
    public void Constructor_EveryDetail_KeepsThem()
    {
        var inner = new InvalidOperationException("inner");

        var exception = new DaxFormatterException(
            "message",
            DaxFormatterFailureReason.HttpError,
            HttpStatusCode.BadGateway,
            "request",
            "service",
            "operation",
            [3, 5],
            inner);

        Assert.Equal(("message", inner), (exception.Message, exception.InnerException));
        Assert.Equal((DaxFormatterFailureReason.HttpError, HttpStatusCode.BadGateway), (exception.Reason, exception.StatusCode));
        Assert.Equal(
            ("request", "service", "operation", "3,5"),
            (exception.RequestId, exception.ServiceRequestId, exception.OperationId, string.Join(",", exception.TextIndexes)));
    }

    [Fact]
    public void Constructor_TextIndexes_KeepsACopy()
    {
        var indexes = new List<int> { 3, 5 };
        var exception = new DaxFormatterException(
            "message", DaxFormatterFailureReason.HttpError, null, null, null, null, indexes, null);

        indexes.Add(7);

        Assert.Equal(2, exception.TextIndexes.Count);
    }

    [Fact]
    public void Constructor_MessageOnly_HasNoDetails()
    {
        var exception = new DaxFormatterException("message");

        Assert.Equal(DaxFormatterFailureReason.Unknown, exception.Reason);
        Assert.Equal((null, null, null), (exception.StatusCode, exception.RequestId, exception.ServiceRequestId));
        Assert.Equal((null, 0), (exception.OperationId, exception.TextIndexes.Count));
    }

    [Fact]
    public void Constructor_MessageAndInnerException_KeepsThem()
    {
        var inner = new InvalidOperationException("inner");

        var exception = new DaxFormatterException("message", inner);

        Assert.Equal(("message", inner), (exception.Message, exception.InnerException));
    }

    [Fact]
    public void Constructor_NoArguments_HasADefaultMessage()
    {
        var exception = new DaxFormatterException();

        Assert.False(string.IsNullOrEmpty(exception.Message));
    }
}
