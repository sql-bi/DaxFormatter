using System.Net;
using System.Net.Http;
using System.Text;

namespace Dax.Formatter.Tests.Fakes;

/// <summary>
/// The answers that a test can give in place of the service.
/// </summary>
internal static class Respond
{
    public const string ServiceRequestId = "fake-azure-ref";

    /// <summary>Answers as <see cref="EchoService"/> does.</summary>
    public static Responder Echo { get; } = (request, _) => Task.FromResult(CreateJson(EchoService.Answer(request.Dax)));

    /// <summary>Never answers: the request ends only when it is cancelled.</summary>
    public static Responder Hang { get; } = async (_, cancellationToken) =>
    {
        await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
        throw new InvalidOperationException("A request that hangs can only be cancelled.");
    };

    public static Responder Json(string json)
    {
        return (_, _) => Task.FromResult(CreateJson(json));
    }

    /// <summary>Answers with the JSON that a function builds from the texts of the request.</summary>
    public static Responder Json(Func<IReadOnlyList<string>, string> answer)
    {
        return (request, _) => Task.FromResult(CreateJson(answer(request.Dax)));
    }

    public static Responder Content(HttpStatusCode status, string content, string mediaType)
    {
        return (_, _) =>
        {
            var response = Create(status);
            response.Content = new StringContent(content, Encoding.UTF8, mediaType);
            return Task.FromResult(response);
        };
    }

    public static Responder Status(HttpStatusCode status, Action<HttpResponseMessage>? configure = null)
    {
        return (_, _) =>
        {
            var response = Create(status);
            response.Content = new ByteArrayContent([]);
            configure?.Invoke(response);
            return Task.FromResult(response);
        };
    }

    public static Responder Throw(Func<Exception> createException)
    {
        return (_, _) => Task.FromException<HttpResponseMessage>(createException());
    }

    /// <summary>Answers with <paramref name="then"/> once <paramref name="gate"/> completes.</summary>
    public static Responder After(Task gate, Responder then)
    {
        return async (request, cancellationToken) =>
        {
            await gate.ConfigureAwait(false);
            return await then(request, cancellationToken).ConfigureAwait(false);
        };
    }

    /// <summary>Does something when a request arrives, such as a cancellation by the caller, then answers.</summary>
    public static Responder OnArrival(Action action, Responder then)
    {
        return (request, cancellationToken) =>
        {
            action();
            return then(request, cancellationToken);
        };
    }

    /// <summary>Answers the first request with the first responder, and so on; the last one answers the rest.</summary>
    public static Responder Sequence(params Responder[] responders)
    {
        return (request, cancellationToken) =>
        {
            var responder = responders[Math.Min(request.Number, responders.Length - 1)];
            return responder(request, cancellationToken);
        };
    }

    private static HttpResponseMessage CreateJson(string json)
    {
        var response = Create(HttpStatusCode.OK);
        response.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return response;
    }

    private static HttpResponseMessage Create(HttpStatusCode status)
    {
        var response = new HttpResponseMessage(status);
        response.Headers.Add("x-azure-ref", ServiceRequestId);
        return response;
    }
}
