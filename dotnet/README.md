# Dax.Formatter [![CI](https://github.com/sql-bi/DaxFormatter/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/sql-bi/DaxFormatter/actions/workflows/dotnet-ci.yml)

`Dax.Formatter` is the .NET client of [DAX Formatter](https://www.daxformatter.com), the service by SQLBI that formats DAX code following the [rules for DAX code formatting](https://www.sqlbi.com/articles/rules-for-dax-code-formatting/) and reports its syntax errors. One call formats one text or the thousands of measures of a model.

## Getting started

Install the package, which targets .NET Standard 2.0 and .NET 10:

```bash
dotnet add package Dax.Formatter
```

Create a client and format a query. The types are in the `Dax.Formatter` namespace.

```csharp
var client = new DaxFormatterClient();

DaxFormatResult result = await client.FormatAsync("EVALUATE FILTER(Customer, Customer[Country] = \"Italy\")");

Console.WriteLine(result.FormattedText);
// EVALUATE
// FILTER ( Customer, Customer[Country] = "Italy" )
```

- The client is thread-safe: create one and reuse it for the life of the application.
- The DAX text goes to the service, which formats it. Nothing else about the model leaves the machine, unless you fill `DaxModelInfo`, whose server and database names are sent only as hashes.
- Every method is asynchronous and takes a `CancellationToken`.

## Usage

### Format the objects of a model

Pass your own objects and a function that returns the DAX of each one. Every result carries its object in `Item`, so nothing has to be matched by position.

```csharp
var options = new DaxFormatOptions { Kind = DaxKind.Expression };

IReadOnlyList<DaxFormatResult<Measure>> results =
    await client.FormatBatchAsync(model.Measures, measure => measure.Expression, options);

foreach (var result in results)
{
    if (result.Succeeded)
    {
        result.Item.Expression = result.FormattedText;
    }
    else
    {
        Console.WriteLine($"{result.Item.Name}: {result.Errors[0].Message}");
    }
}
```

Here `Measure` and `model` stand for the types of your application: any type works.

- `DaxKind.Expression` says that each text is the DAX of one model object without its name: a measure, a calculated column, a calculated table, a calculation item or the body of a user-defined function. The formatted text has no name either.
- `DaxKind.Auto`, the default, sends the text as it is. Use it for a query and for a text that carries its own name, such as `[Margin] := [Sales] - [Cost]`. Don't use it for the DAX of a model object: without a name, the service can take an expression such as `Customer[Name] = "A"` for a definition named `Customer[Name]`.
- The client splits a large call into several requests and sends a few of them at a time. The results come back in the order of the input, all of them or an exception.
- A blank text gets an empty formatted text, without a request.
- `FormatBatchAsync` also takes plain strings: `await client.FormatBatchAsync(texts)`.

### Options

```csharp
var options = new DaxFormatOptions
{
    Kind = DaxKind.Expression,
    LineStyle = DaxLineStyle.ShortLine,
    SpacingStyle = DaxSpacingStyle.NoSpaceAfterFunction,
    SeparatorStyle = DaxSeparatorStyle.SemicolonAndComma,
    ModelInfo = new DaxModelInfo
    {
        ServerType = DaxServerTypes.PowerBIDesktop,
        DatabaseName = "Contoso",
    },
};

DaxFormatResult result = await client.FormatAsync("DIVIDE([Sales Amount]; 1,5)", options);
```

- `LineStyle`: `LongLine` (default) keeps an expression on one line while it fits, `ShortLine` puts each argument on its own line.
- `SpacingStyle`: `SpaceAfterFunction` (default) writes `SUM ( x )`, `NoSpaceAfterFunction` writes `SUM( x )`.
- `SeparatorStyle`: the separators that your DAX uses. `CommaAndPoint` (default) is `DIVIDE ( 1.5, 2 )`, `SemicolonAndComma` is `DIVIDE ( 1,5; 2 )`.
- `ModelInfo` is optional and doesn't change the formatting: the service uses it for anonymous usage statistics. `ServerName` and `DatabaseName` are sent as SHA-256 hashes, never as names.

### Syntax errors

A syntax error is not an exception: it is the result of that text.

```csharp
DaxFormatResult result = await client.FormatAsync("SUMX(Sales, Sales[Amount] *", options);

if (!result.Succeeded)
{
    foreach (DaxSyntaxError error in result.Errors)
    {
        Console.WriteLine($"Line {error.Line + 1}, column {error.Column + 1}: {error.Message}");
    }
}
```

- `Succeeded` is `false`, `FormattedText` is `null`, and `Errors` has at least one error.
- `Line` and `Column` are zero-based and refer to `OriginalText`, the text you passed: lines are separated by `\n`, and `Column` is an index into the UTF-16 characters of the line, so a tab counts as one. An error at the end of the text has the length of the last line as its column.

### Functions

With `DaxKind.Expression`, the body of a [user-defined function](https://learn.microsoft.com/dax/best-practices/dax-user-defined-functions) is formatted like any other expression. The client recognizes it by its parameter list followed by `=>`.

```csharp
var options = new DaxFormatOptions { Kind = DaxKind.Expression };

DaxFormatResult result = await client.FormatAsync(
    "(amount : NUMERIC, rate : NUMERIC) => amount * (1 + rate)", options);

Console.WriteLine(result.FormattedText);
// (
//     amount : NUMERIC,
//     rate : NUMERIC
// ) =>
// amount * ( 1 + rate )
```

### Progress and cancellation

```csharp
var progress = new Progress<DaxFormatProgressInfo>(
    info => Console.WriteLine($"{info.Completed} of {info.Total} expressions formatted"));

IReadOnlyList<DaxFormatResult> results =
    await client.FormatBatchAsync(expressions, options, progress, cancellationToken);
```

`Completed` counts texts, grows as the requests of the call complete, and never decreases. A cancelled call throws `OperationCanceledException`.

### Failures of the service

When the service or the network fails, the call throws `DaxFormatterException` and returns no result.

```csharp
try
{
    IReadOnlyList<DaxFormatResult> results = await client.FormatBatchAsync(expressions);
    Console.WriteLine($"{results.Count} expressions formatted");
}
catch (DaxFormatterException exception)
{
    Console.WriteLine($"{exception.Reason}: {exception.Message}");
    Console.WriteLine($"Request {exception.RequestId}, status {exception.StatusCode}");
}
```

- `Reason` is `Timeout`, `NetworkError`, `HttpError` or `InvalidResponse`. The message names the texts of the failed request by their indexes.
- Before it gives up, the client tries again after a timeout, a network error and the statuses 408, 429, 500, 502, 503 and 504, with a growing wait, and it honors `Retry-After`.
- `RequestId` is the identifier that the client sent in the `DaxFormatter-Request-Id` header; `ServiceRequestId` is the one in the answer, when an answer arrived. Quote both when you report a problem.

### Client options and HttpClient

```csharp
var client = new DaxFormatterClient(new DaxFormatterClientOptions
{
    ApplicationName = "Contoso Model Tools",
    ApplicationVersion = "1.4.0",
    RequestTimeout = TimeSpan.FromSeconds(120),
    MaxRetries = 3,
    MaxConcurrentRequests = 4,
});
```

- `ApplicationName` and `ApplicationVersion` tell the service which application calls it, for usage statistics. They default to the name and the file version of the entry assembly.
- `RequestTimeout` (60 seconds) is the wait for the answer to one request; `MaxRetries` (2) is how many times a request is sent again; `MaxConcurrentRequests` (2) is how many requests of one call are in progress at the same time.

Without an `HttpClient`, all the clients share one that lives as long as the process and doesn't check certificate revocation. To control the handler, the proxy or the certificate checks, pass your own:

```csharp
var client = new DaxFormatterClient(httpClient);
```

The client never changes or disposes an `HttpClient` it is given, and ignores its `BaseAddress`. If its `Timeout` is shorter than `RequestTimeout`, the shorter one wins. With `IHttpClientFactory`, register the client as a typed client: `services.AddHttpClient<DaxFormatterClient>()`.

To replace the client in the tests of your application, derive from it and override `FormatAsync` and the `FormatBatchAsync` that takes strings: both are virtual, the one that takes items calls the one that takes strings, and the parameterless constructor does no work. `DaxFormatResult.Success` and `DaxFormatResult.Failure` create the results.

## Migrating from 1.x

Version 2.0 replaces the whole API. The requests that 1.x exposed are gone: a call takes the text and the options, and returns results.

| 1.x | 2.0 |
|---|---|
| `new DaxFormatterClient(application, version)` | `new DaxFormatterClient(new DaxFormatterClientOptions { ApplicationName = ..., ApplicationVersion = ... })` |
| `FormatAsync(string)`, `FormatAsync(DaxFormatterSingleRequest)` | `FormatAsync(text, options)` |
| `FormatAsync(IEnumerable<string>)`, `FormatAsync(DaxFormatterMultipleRequest)` | `FormatBatchAsync(texts, options)`, or `FormatBatchAsync(items, textSelector, options)` |
| `[x] :=` written before an expression, and removed from the result | `DaxFormatOptions.Kind = DaxKind.Expression` |
| `MaxLineLength`, `SkipSpaceAfterFunctionName` | `DaxFormatOptions.LineStyle`, `DaxFormatOptions.SpacingStyle` |
| `ListSeparator`, `DecimalSeparator` | `DaxFormatOptions.SeparatorStyle` |
| `ServerName`, `DatabaseName`, `ServerVersion`, `DatabaseCompatibilityLevel` on the request | `DaxFormatOptions.ModelInfo`, where the compatibility level is a number |
| The enums `ServerType`, `ServerMode`, `ServerEdition`, `ServerLocation` | Strings; `DaxServerTypes` has the server types |
| `DaxFormatterResponse.Formatted`, empty when there are errors | `DaxFormatResult.FormattedText`, `null` when `Succeeded` is `false` |
| `DaxFormatterError` with nullable `Line` and `Column` | `DaxSyntaxError` |
| `HttpRequestException`, `TaskCanceledException` after 60 seconds | `DaxFormatterException` with a `Reason` |
| `IDaxFormatterClient` | Derive from `DaxFormatterClient`, whose methods are virtual |
| `Dispose()` | Nothing: the client is not disposable |

What else changes:

- The formatted text has no final line break.
- For `DaxKind.Expression`, the position of an error refers to your text, not to the text with `[x] :=` before it.
- A call with many texts no longer fails for its size: the client splits it.
- 2.0 formats DAX only. In 1.x, `FormatAsync(string)` and `FormatAsync(DaxFormatterSingleRequest)` also formatted MDX.
- The package no longer has a `net8.0` target: applications on .NET 8 and 9 use the .NET Standard 2.0 one.

## Service limits

The service, not the client, rejects these texts with a syntax error:

- a function without parameters, such as `() => 1`;
- a function parameter with a default value, such as `(a, b = 1) => a + b`;
- with `DaxKind.Expression`, an expression that has `=>` inside a string or a comment.

## Additional documentation

- [SQLBI Docs](https://docs.sqlbi.com/dax-formatter/): the user documentation of DAX Formatter.
- [Client API](https://github.com/sql-bi/DaxFormatter/blob/master/docs/design/client-api.md): the behavior of the client in detail.
- [REST API](https://github.com/sql-bi/DaxFormatter/blob/master/docs/design/rest-api.md): the contract of the service that the client calls.

## Feedback

Report bugs and request features in [GitHub Issues](https://github.com/sql-bi/DaxFormatter/issues). Report security vulnerabilities privately, as the [security policy](https://github.com/sql-bi/DaxFormatter/blob/master/SECURITY.md) explains.
