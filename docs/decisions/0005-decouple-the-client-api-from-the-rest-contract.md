---
status: accepted
date: 2026-11-22
decision-makers: Alberto Spelta
---

# Decouple the client API from the REST contract

## Context and Problem Statement

`Dax.Formatter` 1.2 exposes the request and response of the REST contract as its public API. A caller builds a request object, gets back responses to match with its own objects by position, and reads the outcome from an empty `Formatted` and a nullable `Errors`. A call waits at most 60 seconds, whatever its size, and a call with thousands of expressions goes out as one request, which can fail for its size or its duration ([sql-bi/DaxFormatter#40](https://github.com/sql-bi/DaxFormatter/issues/40), [sql-bi/Bravo#963](https://github.com/sql-bi/Bravo/issues/963)). The tools that format the objects of a model each write the same code around the client: a `[x] :=` line before every expression, its removal from the result, and the match of results by index. A TypeScript client library and a Python client are planned ([backlog](../backlog.md)). What should the public API of a DAX Formatter client be, so that it stays the same when the service contract changes, and the same across languages?

## Considered Options

* Keep the request and response of the REST contract as the public API, and add splitting and timeouts to it
* Keep the 1.x types and add a layer of helpers for expressions and batches
* A new API on a model of its own: kinds of text, outcomes as values, splitting inside the client

## Decision Outcome

Chosen option: "A new API on a model of its own", because the work that every caller repeats belongs to the client, and an API that doesn't show the wire format can keep its shape when the contract changes and be ported to other languages. It is version 2.0.0 of the package, with breaking changes; [`client-api.md`](../design/client-api.md) describes the model.

- The public types are the client, its options, the options of a call, the result of a text, a syntax error, a progress value and one exception. The types of the wire format are internal.
- A call says what its texts are with a kind. `Auto` sends a text as it is; `Expression` is the DAX of one model object without its name, user-defined functions included. There is no `Query` kind, which would behave like `Auto`, and no `Function` kind: the client recognizes a function body by itself.
- An outcome is a value: a syntax error is part of the result of its text. A failure of the service or of the network is one exception type with a reason, and a cancellation by the caller is the cancellation exception of the language.
- A result can carry the caller's object, so nothing is matched by position. A call returns every result, in the order of the input, or throws.
- The client splits a large call into requests, limits how many are in progress, retries the failures that can be temporary and reports progress. How much a request carries is an internal constant, not an option.
- The client uses only the endpoint that takes several texts, which formats DAX only: the client doesn't format MDX.
- The client doesn't convert line breaks: the service returns those inside strings and comments as they were sent, so converting all of them would change the content of strings. It doesn't halve a request that timed out: the service keeps working on a request that the client abandoned, so halving would double the load when the service is slow.
- The descriptions of the model, which the service uses for statistics, are strings, as in the TypeScript port; the server types that the service knows are constants.
- The shape must exist in every target language and for every consumer of the .NET package: no `init`, `required` or record in public types, which a .NET Framework project on C# 7.3 can't use; no interface next to the client, whose methods are virtual; asynchronous methods only; no new dependency. The .NET package targets `netstandard2.0` and `net10.0`.

### Consequences

* Good, because a caller passes its objects and reads results: the header line, the match by index and the outcome check are no longer written in each tool.
* Good, because a call with thousands of texts no longer depends on one request ([sql-bi/DaxFormatter#40](https://github.com/sql-bi/DaxFormatter/issues/40)).
* Good, because the TypeScript and Python clients get a model to port, with test vectors that don't depend on a language.
* Bad, because every application on 1.x must change its code to move to 2.0.
* Bad, because the client now knows some DAX: where strings, names and comments start and end, and what a function body looks like. A change of the DAX syntax there reaches the client.
* Bad, because `Expression` depends on how the service writes its answer: the client sends a header line, expects it back, and removes it with the indentation that the service adds under a function header. A different answer is a failure, never a wrong text, and the live tests pin the answers.
* Bad, because the client can't work around three limits of the service, which it returns as syntax errors ([`client-api.md`](../design/client-api.md#limits-of-the-service)).
* Bad, because applications that formatted MDX with the single-text methods of 1.x lose it.

### Confirmation

- `PublicApiTests` compares the public surface with `PublicApi.approved.txt` on each target framework, and checks that every public type is in the `Dax.Formatter` namespace: a change of the surface shows up as a change of that file.
- The fixtures under `dotnet/tests/Dax.Formatter.Tests/Fixtures/` hold the request bodies, the recorded answers of the service and the expected results; the tests of every client run them.
- The live tests, run on request, check the recorded answers against the service.
- `ReadmeTests` keeps the code of the package readme equal to code that compiles and runs.

## Pros and Cons of the Options

### Keep the request and response of the REST contract as the public API, and add splitting and timeouts to it

* Good, because no caller has to change its code.
* Bad, because a request object that the client splits no longer maps to one request, so the wire types would stop describing the wire.
* Bad, because a change of the contract stays a change of the public API, as when the metadata enums change form.
* Bad, because the header line, the match by index and the outcome check stay in every caller.

### Keep the 1.x types and add a layer of helpers for expressions and batches

* Good, because existing code keeps working and new code gets the helpers.
* Bad, because two ways to do everything stay in the package, and the old one keeps the failures the new one solves.
* Bad, because the helpers would return the 1.x response types, with their implicit outcome and nullable fields.

### A new API on a model of its own: kinds of text, outcomes as values, splitting inside the client

* Good, because the API is small: two operations, two option types, one result type.
* Good, because it follows the conventions that .NET developers know (see More Information).
* Bad, because it is a breaking change of a published package.

## More Information

- The model and its behavior: [`client-api.md`](../design/client-api.md); the contract it calls: [`rest-api.md`](../design/rest-api.md).
- Names and shapes: [Framework Design Guidelines, names of type members](https://learn.microsoft.com/dotnet/standard/design-guidelines/names-of-type-members) and [enum design](https://learn.microsoft.com/dotnet/standard/design-guidelines/enum), which says not to reserve enum values for future use; [Azure SDK general guidelines](https://azure.github.io/azure-sdk/general_design.html) for client, options and result types; [Task-based Asynchronous Pattern](https://learn.microsoft.com/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap#progress-reporting-optional) for progress; [`IdentityResult`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.identity.identityresult) for `Succeeded` with `Errors`.
- The arrow `=>` belongs to function definitions only: [FUNCTION statement](https://learn.microsoft.com/dax/function-statement-dax).
- The shared `HttpClient`: [Guidelines for using HttpClient](https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines).
- The version of the package: [ADR-0006](0006-write-the-dax-formatter-package-version-by-hand.md).
- How a batch takes the caller's objects, and how the API grows: [ADR-0007](0007-keep-the-batch-api-on-the-callers-items-and-grow-it-only-by-addition.md).
- Revisit if the service gets an endpoint that takes the kind of a text, which would make the header line unnecessary, or if users ask for MDX again.
