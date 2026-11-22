---
status: accepted
date: 2026-11-22
decision-makers: Alberto Spelta
---

# Keep the batch API on the caller's items and grow it only by addition

## Context and Problem Statement

The callers of `Dax.Formatter` format thousands of texts that belong to their own objects. In a model these are measures, calculated columns, calculation items and user-defined functions: objects of different types. One object can also carry several DAX texts: its expression, its format string expression, its detail rows expression.

[ADR-0005](0005-decouple-the-client-api-from-the-rest-contract.md) gave version 2.0 a model of its own. It left open three things, which this record settles:

- how a batch takes the caller's objects;
- how the results come back;
- how the public API grows after 2.0 ships without a new major version, in .NET and in the TypeScript and Python ports.

## Decision Drivers

* A caller never matches results with its objects by position.
* Objects of different types, and several texts of one object, fit in one call.
* What 2.0 ships can't change without a breaking change, so what comes later must be an addition.
* The shape exists in .NET, TypeScript and Python.

## Considered Options

For the input:

* Texts only
* An input type of the library, with an identifier or a state for each text
* The caller's items with a text selector, next to texts
* A builder that queues texts and returns a handle or calls back for each one
* Implicit batching: one call for each text, which the client groups, with a task for each text

For the output:

* A list of results in the order of the input
* A collection type of the library with data about the call
* A stream of results in the order they complete

## Decision Outcome

Chosen options: "The caller's items with a text selector, next to texts" and "A list of results in the order of the input". The common case is one line, each result is tied to the caller's object, and the options not taken can still be added later, when a use appears.

### The shape of 2.0

- `FormatBatchAsync(texts)` and `FormatBatchAsync(items, textSelector)` return an `IReadOnlyList` with one result for each input, in its order. The items version gives each result its item in `Item`.
- Objects of different types go in one call through a small type of the caller, which holds the text and what to do with the result. So do several texts of one object, with one item for each text. The package readme shows such a type.
- A batch is atomic: it returns every result, or throws `DaxFormatterException`. On a failure, `TextIndexes` says which texts the failed request carried and `OperationId` which call failed.
- A syntax error is a value in the result of its text.
- `Task` and `IProgress<T>` serve long calls; there is no stream.

### How the API grows

Each of these is an addition, and none changes what 2.0 does:

- **Settings for each text, such as a kind for each text:**
  - a new input type, `DaxFormatInput`, with nullable settings;
  - new overloads that take it, or an input selector.
  
  A setting of the text wins over the one of the call, and null takes the call's. A lambda that returns a string keeps binding to the existing overload.
- **Default format options on the client:** a property of `DaxFormatterClientOptions`, used when a call passes no options. They replace the options of the call; they never merge with them.
- **An operation identifier chosen by the caller:** a property of `DaxFormatOptions`. With null, each call keeps getting a new identifier.
- **Partial success:**
  - only as an option the caller turns on;
  - the call still throws, and the exception carries the results of the requests that succeeded;
  - a result never gets a third state.
- **Warnings:** a list separate from `Errors`.
- **The end of the range, or a code, of a syntax error:** new properties of `DaxSyntaxError`, with a new constructor.
- **A stream of results:** a new method. An existing method never changes its return type.
- **New enum values:**
  - the client rejects an input value it doesn't know;
  - a caller handles an unknown `DaxFormatterFailureReason` as `Unknown`;
  - in TypeScript and Python, the enums that come out of the client are open types.

### What the API doesn't do

- No option decides whether a call throws, such as a `ThrowOnAnyError`.
- No tuple and no implicit conversion in a public type.
- No style setting for each text: the service applies the styles to a whole request.

### Consequences

* Good, because the common call is one line and no caller matches results by index.
* Good, because each feature in view has a path that keeps 2.x compatible, in source and in binary.
* Bad, because a caller that mixes object types writes a small type of its own.
* Bad, because the list has no room for data about the call: what a caller needs from a failed call travels on the exception.

### Confirmation

- `PublicApiTests` shows every change of the public surface as a change of `PublicApi.approved.txt`. A pull request that changes that file is reviewed against this record.
- After the release of 2.0.0, package validation against that version fails the build on a breaking change ([backlog](../backlog.md)).

## Pros and Cons of the Options

### Texts only

* Good, because it is the simplest call.
* Bad, because settings for each text need new overloads, and the caller matches results with its objects by index. Azure Translator 2.0 broke its API to add a target language for each text.

### An input type of the library, with an identifier or a state for each text

Examples: Azure `TextDocumentInput`, BigQuery `BigQueryInsertRow`, the nested requests of Google's batch methods.

* Good, because settings for each text can be added as properties.
* Bad, because the common case wraps every text.
* Bad, because with an identifier the caller keeps a dictionary to find its objects again. [AIP-233](https://google.aip.dev/233) gives this cost as its reason to report failures by index.

### The caller's items with a text selector, next to texts

* Good, because the common call is one line, as with the selectors of LINQ and Azure Search's `KeyFieldAccessor`.
* Good, because each result carries the caller's object.
* Bad, because objects of different types need a common type, which the caller writes.

### A builder that queues texts and returns a handle or calls back for each one

Examples: Google's `BatchRequest.Queue`, Azure's `BlobBatch`.

* Good, because objects of different types need no common type.
* Bad, because the call has two phases.
* Bad, because a handle can't be used before the batch is sent.
* Bad, because callbacks raise questions of exceptions and threads; the Framework Design Guidelines prefer events to callbacks.

### Implicit batching: one call for each text, which the client groups, with a task for each text

Examples: gax `Batcher`, Cosmos DB bulk mode, Firestore `BulkWriter`.

* Good, because it serves streams without an end.
* Bad, because it needs a flush and a close.
* Bad, because tasks that nobody observes hide failures. Firestore for Node had unhandled rejections ([nodejs-firestore#1282](https://github.com/googleapis/nodejs-firestore/issues/1282)), and the Python client has no task for each write.

### A collection type of the library with data about the call

Example: Azure `AnalyzeSentimentResultCollection`.

* Good, because it has room for data about the call and for lookups, and the [collection guidelines](https://learn.microsoft.com/dotnet/standard/design-guidelines/guidelines-for-collections) prefer a collection class to an interface as a return type.
* Bad, because there is no data about the call to put in it today.
* Bad, because TypeScript and Python use plain arrays and lists.
* Bad, because the test doubles of an application would have to build it.

### A stream of results in the order they complete

* Good, because the first results arrive before the call ends.
* Bad, because Microsoft moved the batch upsert of its vector stores from `IAsyncEnumerable` back to a task ([semantic-kernel#10692](https://github.com/microsoft/semantic-kernel/issues/10692)) and declined streaming for embeddings ([dotnet/extensions#5548](https://github.com/dotnet/extensions/issues/5548)).
* Bad, because on `netstandard2.0` it needs a new dependency.
* Bad, because `IProgress<T>` already serves a progress bar.

## More Information

- The model: [ADR-0005](0005-decouple-the-client-api-from-the-rest-contract.md) and [`client-api.md`](../design/client-api.md).
- Compatibility:
  - Google [AIP-180](https://google.aip.dev/180): a new field must keep the old behavior, and enums in responses are open.
  - [AIP-233](https://google.aip.dev/233): a synchronous batch is atomic, and partial success is opt-in.
  - The .NET [breaking change rules](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/breaking-change-rules.md): default parameter values are part of the contract.
- Exceptions: the [Framework Design Guidelines](https://learn.microsoft.com/dotnet/standard/design-guidelines/exception-throwing) on throwing.
- Throwing or returning the errors of each item: [google-cloud-dotnet#4199](https://github.com/googleapis/google-cloud-dotnet/issues/4199).
- Tuples and records:
  - classes rather than tuples in public APIs ([dotnet/runtime#27939](https://github.com/dotnet/runtime/issues/27939));
  - a tuple becomes a record when it travels between methods ([Choose between tuples, records, structs, and classes](https://learn.microsoft.com/dotnet/csharp/fundamentals/tutorials/choosing-types)).
- Revisit when a consumer needs settings for each text, or the results of a call that failed in part.
