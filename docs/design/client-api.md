# Client API

The model of a DAX Formatter client: what a caller gives it, what it returns, and what it does between the caller and the [REST API](rest-api.md). The model doesn't depend on a language ([ADR-0005](../decisions/0005-decouple-the-client-api-from-the-rest-contract.md)). The .NET library `Dax.Formatter` implements it and gives the names used here; the TypeScript and Python clients will port it ([backlog](../backlog.md)). The TypeScript port inside the MCP server predates this model and doesn't follow it yet.

## Model

- **Client** (`DaxFormatterClient`): formats one text (`FormatAsync`) or several (`FormatBatchAsync`). It is thread-safe, meant to be created once, and holds nothing to dispose. Its methods can be overridden, so a test can replace it without an interface.
- **Client options** (`DaxFormatterClientOptions`), copied and checked when the client is created:

  | Option | Default | Meaning |
  |---|---|---|
  | `ApplicationName`, `ApplicationVersion` | the entry assembly's name and file version | The [caller identity](rest-api.md#caller-identity) |
  | `Endpoint` | `https://api.daxformatter.com/` | The base address of the service |
  | `RequestTimeout` | 60 seconds | The wait for the answer to one attempt of one request |
  | `MaxRetries` | 2 | How many times a request is sent again |
  | `MaxConcurrentRequests` | 2 | How many requests of one call are in progress at the same time |

- **Format options** (`DaxFormatOptions`), copied and checked at the start of each call: `Kind`, `LineStyle`, `SpacingStyle`, `SeparatorStyle`, and the optional `ModelInfo` with the description of the model for the usage statistics. The first value of each enum is its default.
- **Kind** (`DaxKind`): `Auto` sends the text as it is, for a query or a text with its own name; `Expression` is the DAX of one model object without its name, the body of a user-defined function included.
- **Result** (`DaxFormatResult`): `OriginalText`, `Succeeded`, `FormattedText` (absent when `Succeeded` is false) and `Errors` (empty when it is true). A result of the batch operation that takes items also has `Item`, the caller's object.
- **Syntax error** (`DaxSyntaxError`): `Line`, `Column`, `Message`, with value equality.
- **Progress** (`DaxFormatProgressInfo`): `Completed` and `Total`, as numbers of texts.
- **Failure** (`DaxFormatterException`): `Reason` (`Timeout`, `NetworkError`, `HttpError`, `InvalidResponse`, or `Unknown` when the exception was not created by the client), `StatusCode`, `RequestId`, `ServiceRequestId`, `OperationId`, and `TextIndexes`, the indexes of the texts that the failed request carried.
- **Enums** can gain values in a minor version. The client rejects an input value that it doesn't know; a caller handles an unknown failure reason as `Unknown`. In TypeScript and Python the enums that come out of the client are open types.
- How a batch takes the caller's objects, and how the model grows without breaking: [ADR-0007](../decisions/0007-keep-the-batch-api-on-the-callers-items-and-grow-it-only-by-addition.md).

## Outcomes

- A syntax error is a value: the result of that text has `Succeeded` false and at least one error.
- A failure of the service or of the network throws `DaxFormatterException`. A batch returns every result, in the order of the input, or throws: it never returns a part.
- A cancellation by the caller throws the cancellation exception of the language. It wins over a failure that happens at the same time.
- A wrong argument throws before any request: a null text, a null text in a batch (the message has its index), a text selector that returns null, an option outside its enum.

## Texts

- A blank text, empty or made of white space, is not sent: its result has an empty formatted text. A batch without texts, or with blank texts only, sends no request.
- The formatted text is the service's, without the final line break. Line breaks are not converted: the service writes `\r\n` and returns the line breaks inside strings and comments as they were sent.
- `Auto`: the text travels as it is, and the position of an error is the one the service reports, limited to the text.
- `Expression`: the client writes a header line before the text and removes it from the answer.
  - The header is `FUNCTION x =` for a function body and `[x] :=` for anything else, followed by a line feed, so that the lines of the text keep their columns.
  - A text is a function body when, after white space and comments, it starts with a parameter list in balanced parentheses followed by `=>`. Parentheses inside strings, names and comments don't count. DAX has the arrow only in [function definitions](https://learn.microsoft.com/dax/function-statement-dax).
  - The answer must start with the header: the client removes it with the spaces and the one line break that follow it. An answer that doesn't start with it is an `InvalidResponse` failure, so a text that the client can't verify never reaches the caller.
  - Under a function header the service indents every line by four more spaces, and writes the parameter list either on the header line or, when a comment comes first, on the next one. The client removes four spaces from the start of each line after the header line. A line that starts inside a string, a quoted or bracketed name or a block comment is content: the service returns it as it was sent, and the client leaves it alone.

## Error positions

`Line` and `Column` are zero-based and refer to the text that the caller gave. Lines are separated by `\n`. `Column` is an index into the line in the unit that the strings of the language are indexed by: UTF-16 code units in .NET and TypeScript, code points in Python. It equals the length of the line, without its `\r`, when the error is at its end.

The client converts the [position that the service reports](rest-api.md#response):

- for `Expression`, the line minus one, because of the header line; an error on the header line becomes line 0, column 0;
- the column from the service's count, where a tab reaches the next multiple of 8 and `\r` has no width, to the index in the line;
- an error past the last line, as the service reports the end of the input, becomes the end of the last line.

## Requests

- A call is split into requests of at most 250 texts and 250,000 characters; a longer text travels alone. The two numbers are internal constants, not yet measured ([backlog](../backlog.md)).
- The header lines are added when a request is written, and its body is kept as bytes for its retries.
- At most `MaxConcurrentRequests` requests are in progress. Each result is stored at the index of its text.
- Progress is reported after each request, and once before the first one for the blank texts. The receiver gets the counts one at a time and in increasing order, never under a lock of the client. A .NET `Progress<T>` without a synchronization context runs its handler on the thread pool, where two reports can run in the opposite order.
- Retry: after a timeout, a network error and the statuses 408, 429, 500, 502, 503 and 504. Any other status and an invalid response fail at once. The wait starts at 0.8 seconds and doubles up to 60, varied by a quarter either way; a longer `Retry-After`, in seconds or as a date measured from the `Date` header of the same response, replaces it, and one longer than `RequestTimeout` fails the request at once. Each attempt sends the same body and the same request identifier.
- The first request that fails after its retries stops the others, and the call fails with it. The message names the first and the last index of the texts of that request; `TextIndexes` lists them all, without the blank texts, which are never sent.
- A request that timed out is not split in two: the service keeps working on an abandoned request, so smaller requests would add load when it is slow.
- Invalid responses: status 204, a body that is not the documented JSON, a number of results other than the number of texts sent, a null result. A UTF-8 byte order mark before the JSON is skipped. In a result, a null `errors` is no errors, and a null `formatted` without errors is an empty text; `formatted` is not read when there are errors.

## Identity and transport

- Each request has the [caller identity](rest-api.md#caller-identity) in its body, and the headers `User-Agent`, `DaxFormatter-Request-Id` (one for each request, the same in its retries) and `DaxFormatter-Operation-Id` (one for each call) ([headers](rest-api.md#endpoints)).
- `User-Agent` is `<application>/<version> Dax.Formatter/<library version> (<runtime>)`. The application name and version are cleaned into HTTP tokens, so `DAX Studio` becomes `DAX-Studio`; the body keeps them as they are.
- Without an HTTP client from the caller, all the clients share one that lives as long as the process, has no timeout of its own and doesn't check certificate revocation; on .NET 10 it replaces its pooled connections every 5 minutes, so that a DNS change is seen ([guidelines](https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines)). Nothing is created before the first call.
- On .NET Framework the shared client is the plain default one, so it opens at most [`ServicePointManager.DefaultConnectionLimit`](https://learn.microsoft.com/dotnet/api/system.net.servicepointmanager.defaultconnectionlimit) connections to the service, 2 unless the application changes it. A request beyond that waits for a connection while its timeout runs, which matters with `MaxConcurrentRequests` above 2 or with several calls at the same time.
- An HTTP client from the caller is never changed or disposed. Its base address is ignored: every request has an absolute URI built from `Endpoint`. Its own timeout applies too, so the shorter one wins, and is reported as `Timeout`.
- The server and database names of `ModelInfo` leave the client only as [hashes](rest-api.md#hashing-of-server-and-database-names).

## Limits of the service

The client returns these as the syntax errors that the service reports. They were observed on 2026-11-22 and the live tests check that they still hold:

1. A function without parameters, `() => 1`, is a syntax error, though [valid DAX](https://learn.microsoft.com/dax/best-practices/dax-user-defined-functions).
2. A function parameter with a default value, `(a, b = 1) => a + b`, is a syntax error.
3. With `Expression`, an expression that is not a function and has `=>` inside a string or a comment is a syntax error on the header line, which the client reports at line 0, column 0.

## Code and tests

- .NET: `dotnet/src/Dax.Formatter`. Public types are in the root folder, one in each file; internal ones in a folder for each responsibility: `Configuration/` (copies of the options, request limits, caller identity), `Wire/` (request and response bodies, name hashing), `Expressions/` (the DAX text scanner, the function recognizer, the header, its removal, error positions), `Results/`, `Batching/` (planning, workers, progress), `Transport/` (one attempt, the retry loop and its policy, the shared HTTP client), `Polyfills/` (what `netstandard2.0` lacks).
- The cases of the wire format are JSON files in `dotnet/tests/Dax.Formatter.Tests/Fixtures/`, written so that every client can run them: request bodies, name hashes, function bodies, and recorded answers of the service with the result expected from each. They move to `spec/` with the shared contract ([backlog](../backlog.md)).
- The tests run offline against a fake HTTP handler. The tests in `Live/` call the service and run only on request ([`dotnet/AGENTS.md`](../../dotnet/AGENTS.md)).
- `PublicApiTests` pins the public surface in `PublicApi.approved.txt`; `ReadmeTests` pins the code of the package readme to code that runs.
