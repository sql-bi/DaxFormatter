# REST API

The public REST contract of the DAX Formatter service, as the .NET client and the TypeScript port inside the MCP server use it. The service is maintained outside this repository; this document is the only place in the repository that states its contract.

## Endpoints

| Method | URL | Body | Response |
|---|---|---|---|
| `POST` | `https://api.daxformatter.com/api/daxtextformat` | a request whose `dax` is a string | one result |
| `POST` | `https://api.daxformatter.com/api/daxtextformatmulti` | a request whose `dax` is an array of strings | an array of results, one per expression, in the order sent |

- No authentication.
- Request: `Content-Type: application/json`, UTF-8; `Accept: application/json`. The .NET client also sends `Accept-Encoding: GZip` and decompresses gzip responses.
- The .NET client uses either endpoint (`DaxFormatterSingleRequest`, `DaxFormatterMultipleRequest`); the MCP server always uses the multiple one.

## Request

A JSON object with camelCase names. Both clients omit a field whose value is null or not set.

| Field | Type | Meaning | Clients |
|---|---|---|---|
| `dax` | string, or array of strings | The DAX to format: one expression on the single endpoint, a list on the multiple one. | always sent |
| `maxLineLength` | integer | Line style: `0` long lines, `1` short lines. | always sent, default `0`; in .NET a caller can set it to null |
| `skipSpaceAfterFunctionName` | boolean | `false`: a space between a function name and its parenthesis, `SUM ( ... )`; `true`: none, `SUM( ... )`. | always sent, default `false`; in .NET a caller can set it to null |
| `listSeparator` | string, one character | Separator of arguments and list items. | always sent, default `,` |
| `decimalSeparator` | string, one character | Decimal point in numbers. | always sent, default `.` |
| `serverName` | string | Hash of the server name ([hashing](#hashing-of-server-and-database-names)). | when given |
| `databaseName` | string | Hash of the database name ([hashing](#hashing-of-server-and-database-names)). | when given |
| `serverType` | string | Kind of server the DAX comes from ([enums](#enums)). | when given |
| `serverMode` | number or string | Analysis Services server mode ([enums](#enums)). | when given |
| `serverEdition` | number or string | Analysis Services edition ([enums](#enums)). | when given |
| `serverLocation` | number or string | On premises or Azure ([enums](#enums)). | when given |
| `serverVersion` | string | Server version, for example `14.0.800.192`. | when given |
| `databaseCompatibilityLevel` | string | Compatibility level of the database, for example `1500`. | when given |
| `callerApp` | string | Name of the calling application ([caller identity](#caller-identity)). | when known |
| `callerVersion` | string | Version of the calling application ([caller identity](#caller-identity)). | when known |

The model metadata, from `serverName` to `databaseCompatibilityLevel`, doesn't change the formatting: the service uses it for usage statistics.

Example, as the TypeScript port sends it:

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

Here `serverName` is the hash of `MyServer`, and the .NET client would send `serverMode` as `2` ([enums](#enums)).

## Enums

`serverType` is a string in both clients, with these values:

| .NET `ServerType` and TypeScript `ServerType` member | Wire value |
|---|---|
| `AnalysisServices` | `SSAS` |
| `PowerBIDesktop` | `PBI Desktop` |
| `PowerBIReportServer` | `PBI Report Server` |
| `PowerPivot` | `PowerPivot` |
| `SSDT` | `SSDT` |
| `AzureAnalysisServices` | `AzureAS` |
| `PowerBIService` | `PBI Service` |
| `Offline` | `Offline` |

`serverMode`, `serverEdition` and `serverLocation` have the same members in both clients, but their wire form differs: the .NET client sends the member's number, the TypeScript port sends the member's name as a string. This divergence is known debt, not a rule to copy ([backlog](../backlog.md#known-debt)).

| Field | Member: .NET number, TypeScript string |
|---|---|
| `serverMode` | `Multidimensional`: 0; `SharePoint`: 1; `Tabular`: 2; `Default`: 3 |
| `serverEdition` | `Standard`: 0; `Standard64`: 1; `Enterprise`: 2; `Enterprise64`: 3; `Developer`: 4; `Developer64`: 5; `Evaluation`: 6; `Evaluation64`: 7; `LocalCube`: 8; `LocalCube64`: 9; `BusinessIntelligence`: 10; `BusinessIntelligence64`: 11; `EnterpriseCore`: 12; `EnterpriseCore64`: 13 |
| `serverLocation` | `OnPremise`: 0; `Azure`: 1 |

## Response

A result is a JSON object:

| Field | Type | Meaning |
|---|---|---|
| `formatted` | string | The formatted DAX, with `\r\n` line breaks. When the expression has syntax errors, the service returns an empty string; both clients also accept `null`. |
| `errors` | array of objects | Syntax errors in the expression, empty when it is valid. |
| `errors[].line` | integer | Line of the error, as the service reports it. |
| `errors[].column` | integer | Column of the error, as the service reports it. |
| `errors[].message` | string | Description of the error. |

Both clients model every field as nullable, so a client must accept `null` for any of them. The TypeScript port turns a `null` or missing `errors` into an empty array; the .NET client returns it as `null`.

From the multiple endpoint:

```json
[
  { "formatted": "EVALUATE\r\nSales\r\n", "errors": [] },
  { "formatted": "", "errors": [ { "line": 1, "column": 0, "message": "Syntax error, expected: ..." } ] }
]
```

## Errors

- DAX syntax errors are not HTTP errors: the service answers `200 OK` with the errors in the result.
- Any other failure is a non-success HTTP status, for example `500` for a body that is not valid JSON, or a transport failure with no response at all.
- The .NET client throws `HttpRequestException` for a non-success status or a transport failure, and `TaskCanceledException` when a request takes longer than 60 seconds.
- The TypeScript port throws an `Error` with the status for a non-success status, rejects with the error of `fetch` for a transport failure, and aborts a request that takes longer than its `timeoutMs` option, 60 seconds by default, which the MCP server keeps.
- The MCP tool returns each of these failures as a tool error.

## Hashing of server and database names

`serverName` and `databaseName` never carry the names themselves. The client sends the SHA-256 hash of the name's UTF-8 bytes, as 64 lowercase hexadecimal characters, so that the service can count distinct servers and databases without learning their names.

- An empty name is hashed too: `""` becomes `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855`. A missing name is not sent.
- The .NET models hash on assignment: reading `ServerName` or `DatabaseName` back returns the hash. The TypeScript port hashes while it builds the request.
- Both clients test the same vectors: `SecurityHelpersTests` in .NET and `hash.test.ts` in TypeScript.

## Caller identity

`callerApp` and `callerVersion` name the application that calls the service and its version, for usage statistics.

| Client | `callerApp` | `callerVersion` |
|---|---|---|
| .NET `DaxFormatterClient` | the `application` argument of the constructor, or else the entry assembly's name | the `version` argument, or else the entry assembly's file version |
| TypeScript port | the `appName` option | the `appVersion` option |
| MCP server | `daxformatter-mcp-local/<client name>`, or `daxformatter-mcp-local` when the MCP client doesn't send its name | `<server version>/<client version>`, or the server version alone |

The .NET client writes both fields from the client instance into every request, over any value set on the request object. The MCP server takes the client's name and version from the MCP `initialize` request.

## Implementations

- .NET: `dotnet/src/Dax.Formatter`, the request and response models in `Models/`, the enums in `AnalysisServices/`, the JSON converters in `Serialization/`, the hashing in `Security/`, the HTTP calls in `Client/Http/`.
- TypeScript: `typescript/packages/daxformatter-mcp/src/daxformatter/`, the types in `types.ts`, the wire form in `serialize.ts`, the hashing in `hash.ts`, the HTTP calls in `client.ts`; the options that the MCP tool exposes are in the zod schemas of `src/format-tool.ts`.
