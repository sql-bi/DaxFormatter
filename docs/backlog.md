# Backlog

Reports and requests from users stay in [GitHub Issues](https://github.com/sql-bi/DaxFormatter/issues); an item here links its issue instead of copying it.

## Now
Reorganizing the repository by language and releasing each package with its own version and tags: [`plans/language-first-layout.md`](plans/language-first-layout.md).

## Next
- npm trusted publishing for the MCP server release: the release workflow publishes with a long-lived npm token, which can leak and must be rotated by hand; with trusted publishing npm issues a short-lived credential to the workflow and adds provenance on its own. Done when the trusted publisher on npm names `typescript-release-daxformatter-mcp.yml` (so after the move), the release job runs on Node 22.14 or later with npm 11.5.1 or later, a release publishes without the token, and the Maintainer deletes the token.
- MCP registry entry for the stdio server: AI clients that browse the registry find it there. Done when a `server.json` describes the npm package with `repository.subfolder` set to `typescript/packages/daxformatter-mcp`, the server appears in the registry, and the release guide says how to update the entry.

## Later
- OpenAPI contract in `spec/`, with test vectors shared by all clients: one definition of the REST contract (see Known debt).
- TypeScript client library in `typescript/packages/daxformatter`, extracted from the port inside the MCP package and used by it.
- npm workspaces rooted at `typescript/`, so the npm packages share one lockfile and one install; needed once there are two npm packages.
- Python client in `python/`.

## Known debt
- Enum wire format diverges between the clients: the .NET client sends `ServerMode`, `ServerEdition` and `ServerLocation` as numbers (System.Text.Json web defaults, no converter), while the TypeScript port sends them as strings; `ServerType` is a string in both (`EnumMember` values through `JsonEnumMemberConverter` in .NET). Cost: the service receives two forms of the same fields, and a new client has no rule to follow; changing the .NET form changes the wire format of a published package. Repay with the OpenAPI contract, which fixes one form for every client.
- The REST contract is restated in each client: the .NET request and response models, the TypeScript port's types and serializer, and the zod schemas of the MCP tool for the options it exposes. Cost: every contract change touches each copy, and the copies already drift (the enum wire format above). Repay with the OpenAPI contract and its shared test vectors.
- The getting-started sample references `Dax.Formatter` 1.0.0, while nuget.org has 1.2.2. Cost: the sample builds against a package from 2021, not the one users install today. Repay in the next change to the sample.
- The .NET tests call the live service: `DaxFormatterClientTests` uses a real `DaxFormatterClient`, so .NET CI depends on the service and the network, and an outage fails unrelated pull requests. The MCP tests inject a mock `fetch` and run offline. Repay with the shared test vectors: unit tests against recorded responses, and the live tests in a separate job.
- The MCP server says that `formatted` is null for invalid DAX: the tool description in `typescript/packages/daxformatter-mcp/src/format-tool.ts` ("Invalid DAX: `formatted` is null") and the comment of `formatted` in `src/daxformatter/types.ts`, while the service returns an empty string ([REST API, Response](design/rest-api.md#response)). Cost: an AI client that follows the tool description checks for null and misses the empty string. Repay with a change to the tool description and the type comment, outside the language-first layout plan: AI clients read the tool description, and the change raises the MCP server's version.
