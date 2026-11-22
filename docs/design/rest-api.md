# REST API

The public REST contract of the DAX Formatter service, as observed from outside, and how the two clients of this repository use it: the .NET client and the TypeScript port inside the MCP server. The service is maintained outside this repository; this document is the only place in the repository that states its contract. What the .NET client does around the contract, such as splitting and retries, is in [`client-api.md`](client-api.md).

## Endpoints

| Method | URL | Body | Response |
|---|---|---|---|
| `POST` | `https://api.daxformatter.com/api/daxtextformat` | a request whose `dax` is a string | one result |
| `POST` | `https://api.daxformatter.com/api/daxtextformatmulti` | a request whose `dax` is an array of strings | an array of results, one per text, in the order sent |

- No authentication.
- Request: `Content-Type: application/json`, UTF-8; `Accept: application/json`.
- The multiple endpoint formats DAX only; the single one also formats MDX. The .NET client and the MCP server use only the multiple one.
- A request whose `dax` array is empty gets `204 No Content`.
- Responses are not compressed, and each has an `x-azure-ref` header that identifies the request on the service side.
- The .NET client adds three headers, which don't change the answer and let a request be traced:

  | Header | Value |
  |---|---|
  | `User-Agent` | `<application>/<version> Dax.Formatter/<library version> (<runtime>)` |
  | `DaxFormatter-Request-Id` | a GUID for the request, the same in its retries |
  | `DaxFormatter-Operation-Id` | a GUID for the client call, the same in all its requests |

## Request

A JSON object with camelCase names. Both clients omit a field whose value is null or not set.

| Field | Type | Meaning | Clients |
|---|---|---|---|
| `dax` | string, or array of strings | The DAX to format: one text on the single endpoint, a list on the multiple one. | always sent |
| `maxLineLength` | integer | Line style: `0` long lines, `1` short lines. | always sent, default `0` |
| `skipSpaceAfterFunctionName` | boolean | `false`: a space between a function name and its parenthesis, `SUM ( ... )`; `true`: none, `SUM( ... )`. | always sent, default `false` |
| `listSeparator` | string, one character | Separator of arguments and list items. | always sent, default `,` |
| `decimalSeparator` | string, one character | Decimal point in numbers. | always sent, default `.` |
| `serverName` | string | Hash of the server name ([hashing](#hashing-of-server-and-database-names)). | when given |
| `databaseName` | string | Hash of the database name ([hashing](#hashing-of-server-and-database-names)). | when given |
| `serverType` | string | Kind of server the DAX comes from ([enums](#enums)). | when given |
| `serverMode` | string | Analysis Services server mode ([enums](#enums)). | when given |
| `serverEdition` | string | Analysis Services edition ([enums](#enums)). | when given |
| `serverLocation` | string | On premises or Azure ([enums](#enums)). | when given |
| `serverVersion` | string | Server version, for example `16.0.143.21`. | when given |
| `databaseCompatibilityLevel` | string | Compatibility level of the database, for example `1600`. | when given |
| `callerApp` | string | Name of the calling application ([caller identity](#caller-identity)). | when known |
| `callerVersion` | string | Version of the calling application ([caller identity](#caller-identity)). | when known |

The model metadata, from `serverName` to `databaseCompatibilityLevel`, doesn't change the formatting: the service uses it for usage statistics. The options hold for the whole request.

Example:

```json
{
  "dax": ["EVALUATE Sales"],
  "maxLineLength": 0,
  "skipSpaceAfterFunctionName": false,
  "listSeparator": ",",
  "decimalSeparator": ".",
  "serverName": "5e3e900585e7834b034d4f17187e521b3c7b7de0d1637f79de4b63717d4f0783",
  "serverMode": "Tabular",
  "callerApp": "daxformatter-mcp-local/example-client",
  "callerVersion": "0.2.1-beta/1.0.0"
}
```

Here `serverName` is the hash of `MyServer`.

## Enums

`serverType`, `serverMode`, `serverEdition` and `serverLocation` are strings, which the service stores as they come. Both clients have an enum for each. For `serverMode`, `serverEdition` and `serverLocation` the .NET enums `DaxServerMode`, `DaxServerEdition` and `DaxServerLocation` send the name of the member, which is the name in the Analysis Services enums `ServerMode`, `ServerEdition` and `ServerLocation`. The TypeScript port doesn't have `StandardDeveloper` and `StandardDeveloper64`.

| .NET `DaxServerType` member, TypeScript `ServerType` member | `serverType` |
|---|---|
| `AnalysisServices` | `SSAS` |
| `PowerBIDesktop` | `PBI Desktop` |
| `PowerBIReportServer` | `PBI Report Server` |
| `PowerPivot` | `PowerPivot` |
| `Ssdt` in .NET, `SSDT` in TypeScript | `SSDT` |
| `AzureAnalysisServices` | `AzureAS` |
| `PowerBIService` | `PBI Service` |
| `Offline` | `Offline` |

| Field | Values |
|---|---|
| `serverMode` | `Multidimensional`, `SharePoint`, `Tabular`, `Default` |
| `serverEdition` | `Standard`, `Standard64`, `Enterprise`, `Enterprise64`, `Developer`, `Developer64`, `Evaluation`, `Evaluation64`, `LocalCube`, `LocalCube64`, `BusinessIntelligence`, `BusinessIntelligence64`, `EnterpriseCore`, `EnterpriseCore64`, `StandardDeveloper`, `StandardDeveloper64` |
| `serverLocation` | `OnPremise`, `Azure` |

## Response

A result is a JSON object:

| Field | Type | Meaning |
|---|---|---|
| `formatted` | string | The formatted DAX. When the text has syntax errors, the service returns an empty string; both clients also accept `null`. |
| `errors` | array of objects | Syntax errors in the text, empty when it is valid. |
| `errors[].line` | integer | Zero-based line of the error in the text sent, where lines are separated by `\n`. |
| `errors[].column` | integer | Zero-based column of the error in that line. |
| `errors[].message` | string | Description of the error. |

Both clients accept `null` for any field, and read a `null` or missing `errors` as no errors.

From the multiple endpoint:

```json
[
  { "formatted": "EVALUATE\r\nSales\r\n", "errors": [] },
  { "formatted": "", "errors": [ { "line": 1, "column": 0, "message": "Syntax error, expected: ..." } ] }
]
```

What the service does with a text:

- The formatted text has `\r\n` line breaks and ends with one. Line breaks inside strings and block comments come back as they were sent. A blank text comes back as an empty string without errors.
- A text of the form `name = expression` or `name := expression` is a definition, and the name is kept. A text without a name can be taken for one: `Customer[Name] = "A"` is read as the definition of `Customer[Name]`. A client that formats the DAX of a model object therefore sends it under a name, as `[x] :=` and a line feed before the text, and gets `[x] :=`, a line break and the formatted text.
- A function is sent as `FUNCTION x =`, a line feed and its body, a parameter list followed by `=>`. The service answers with the parameter list on the line of `FUNCTION x =`, or on the next line when a comment comes first, and indents every following line by four more spaces, except the lines that start inside a string or a block comment.
- The column of an error counts UTF-16 code units; a tab counts up to the next multiple of 8, and `\r` has no width.
- An error at the end of the input has the line after the last one and column 0.
- Known limits: a function without parameters, `() => 1`, and a function parameter with a default value, `(a, b = 1) => a + b`, are syntax errors; so is an expression sent under `[x] :=` that has `=>` inside a string or a comment, at line 0.

## Errors

- DAX syntax errors are not HTTP errors: the service answers `200 OK` with the errors in the result.
- Any other failure is a non-success HTTP status, for example `500` for a body that is not valid JSON, or a transport failure with no response at all.
- The .NET client throws `DaxFormatterException` with a reason for a failure, a timeout or a response it can't use, after its retries ([client API](client-api.md#requests)).
- The TypeScript port throws an `Error` with the status for a non-success status, rejects with the error of `fetch` for a transport failure, and aborts a request that takes longer than its `timeoutMs` option, 60 seconds by default, which the MCP server keeps.
- The MCP tool returns each of these failures as a tool error.

## Hashing of server and database names

`serverName` and `databaseName` never carry the names themselves. The client sends the SHA-256 hash of the name's UTF-8 bytes, as 64 lowercase hexadecimal characters, so that the service can count distinct servers and databases without learning their names.

- An empty name is hashed too: `""` becomes `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855`. A missing name is not sent.
- The .NET client hashes when it copies the options of a call, and keeps only the hashes. The TypeScript port hashes while it builds the request.
- Both clients test the same vectors: the fixture `name-hashes.json` in .NET and `hash.test.ts` in TypeScript.

## Caller identity

`callerApp` and `callerVersion` name the application that calls the service and its version, for usage statistics.

| Client | `callerApp` | `callerVersion` |
|---|---|---|
| .NET `DaxFormatterClient` | the `ApplicationName` option, or else the entry assembly's name | the `ApplicationVersion` option, or else the entry assembly's file version |
| TypeScript port | the `appName` option | the `appVersion` option |
| MCP server | `daxformatter-mcp-local/<client name>`, or `daxformatter-mcp-local` when the MCP client doesn't send its name | `<server version>/<client version>`, or the server version alone |

The .NET client also sends the same identity in `User-Agent` ([endpoints](#endpoints)); the body fields stay the identity of reference. The MCP server takes the client's name and version from the MCP `initialize` request.

## Implementations

- .NET: `dotnet/src/Dax.Formatter`, the request and response bodies, their JSON and the hashing in `Wire/`, the header lines and the error positions in `Expressions/`, the headers and the HTTP calls in `Transport/`, the caller identity in `Configuration/`; the cases of the wire format in `dotnet/tests/Dax.Formatter.Tests/Fixtures/`.
- TypeScript: `typescript/packages/daxformatter-mcp/src/daxformatter/`, the types in `types.ts`, the wire form in `serialize.ts`, the hashing in `hash.ts`, the HTTP calls in `client.ts`; the options that the MCP tool exposes are in the zod schemas of `src/format-tool.ts`.
