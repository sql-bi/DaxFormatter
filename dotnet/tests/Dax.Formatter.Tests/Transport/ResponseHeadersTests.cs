using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Dax.Formatter.Transport;
using Xunit;

namespace Dax.Formatter.Tests.Transport;

public class ResponseHeadersTests
{
    private static readonly DateTimeOffset s_serviceTime = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void GetRetryAfter_Seconds_ReturnsThem()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(7));

        Assert.Equal(TimeSpan.FromSeconds(7), ResponseHeaders.GetRetryAfter(response));
    }

    [Fact]
    public void GetRetryAfter_Date_MeasuresItFromTheDateOfTheResponse()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        response.Headers.Date = s_serviceTime;
        response.Headers.RetryAfter = new RetryConditionHeaderValue(s_serviceTime.AddSeconds(30));

        Assert.Equal(TimeSpan.FromSeconds(30), ResponseHeaders.GetRetryAfter(response));
    }

    [Fact]
    public void GetRetryAfter_DateInThePast_ReturnsZero()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        response.Headers.Date = s_serviceTime;
        response.Headers.RetryAfter = new RetryConditionHeaderValue(s_serviceTime.AddSeconds(-30));

        Assert.Equal(TimeSpan.Zero, ResponseHeaders.GetRetryAfter(response));
    }

    [Fact]
    public void GetRetryAfter_DateWithoutTheDateOfTheResponse_ReturnsNull()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(s_serviceTime.AddSeconds(30));

        Assert.Null(ResponseHeaders.GetRetryAfter(response));
    }

    [Fact]
    public void GetRetryAfter_NoHeader_ReturnsNull()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

        Assert.Null(ResponseHeaders.GetRetryAfter(response));
    }

    [Fact]
    public void GetServiceRequestId_Header_ReturnsItsValue()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Add("x-azure-ref", "20260115T100000Z-abc");

        Assert.Equal("20260115T100000Z-abc", ResponseHeaders.GetServiceRequestId(response));
    }

    [Fact]
    public void GetServiceRequestId_NoHeader_ReturnsNull()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);

        Assert.Null(ResponseHeaders.GetServiceRequestId(response));
    }
}
