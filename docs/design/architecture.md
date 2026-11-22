# Architecture

## Context

DAX Formatter is a service that formats DAX expressions and reports their syntax errors. This repository holds two clients of its REST API ([`rest-api.md`](rest-api.md)): a .NET client library that applications embed, and a stdio MCP server that AI clients start on the user's machine with `npx`. The service, its REST API and its remote MCP server are maintained outside this repository; how to connect an AI client to either MCP server is documented only on SQLBI Docs ([remote](https://docs.sqlbi.com/dax-formatter/mcp-http), [stdio](https://docs.sqlbi.com/dax-formatter/mcp-stdio)).

## Building blocks

Each package lives in its language folder and builds with its own toolchain ([ADR-0001](../decisions/0001-organize-the-repository-by-language.md)).

- `Dax.Formatter`, in `dotnet/`: `DaxFormatterClient` formats one DAX text or thousands in one call, splits a large call into requests, retries them and returns one result for each text ([`client-api.md`](client-api.md), [ADR-0005](../decisions/0005-decouple-the-client-api-from-the-rest-contract.md)). Its public API doesn't show the wire format. All the clients share one `HttpClient`, unless the application passes its own. One solution holds the library and its tests ([ADR-0003](../decisions/0003-keep-one-net-solution-with-role-folders.md)); the examples are in the package readme, checked by the tests.
- `@sqlbi/daxformatter-mcp`, in `typescript/packages/daxformatter-mcp/`: one read-only tool, `format_dax`, which sends every expression in one request to the multiple endpoint, through the package's own TypeScript port of the client in `src/daxformatter/`, not published on its own.
- The REST contract is implemented twice, once per client; [`rest-api.md`](rest-api.md) is its only description. The TypeScript port doesn't follow the client model of [`client-api.md`](client-api.md) yet ([backlog](../backlog.md)).

## Cross-cutting rules

- Privacy: the DAX text goes to the service, where the formatting happens; server and database names are hashed by the client before they leave it ([hashing](rest-api.md#hashing-of-server-and-database-names)).
- Caller identity: each request names the calling application and its version, for usage statistics; the MCP server adds the AI client's name and version ([caller identity](rest-api.md#caller-identity)).
- Errors: DAX syntax errors are normal results; only transport failures, timeouts, non-success HTTP statuses and responses that a client can't use are errors ([errors](rest-api.md#errors)). The .NET client reports them all as `DaxFormatterException`, and a cancellation by the caller as `OperationCanceledException` ([outcomes](client-api.md#outcomes)).
- Tests: both test suites run offline: the .NET tests answer from a fake `HttpMessageHandler`, the MCP tests replace `fetch`. The .NET tests that call the live service are skipped unless asked for ([`dotnet/AGENTS.md`](../../dotnet/AGENTS.md)); they check the answers recorded in the fixtures against the service.

## Deployment

Each package has its own version, tags and release workflow ([ADR-0004](../decisions/0004-version-tag-and-release-each-package-independently.md)). Nerdbank.GitVersioning (NBGV) reads the version from the package's `version.json`; the height counts the commits that touch its path filters, and only `master` gets a version without a commit suffix.

- `Dax.Formatter`: `dotnet/version.json` holds the full version, three numbers written by hand, such as `2.0.0`, which is the version of the package ([ADR-0006](../decisions/0006-write-the-dax-formatter-package-version-by-hand.md)); the height, counted on the path filter `./src/Dax.Formatter`, is only the fourth number of the file version. Stamped by the `Nerdbank.GitVersioning` MSBuild package that `dotnet/Directory.Build.props` references; released by `dotnet-release-dax-formatter.yml` to nuget.org, with Trusted Publishing.
- `@sqlbi/daxformatter-mcp`: `typescript/packages/daxformatter-mcp/version.json` holds two numbers and NBGV adds the height as the third; the path filter is `.`, so any change in the package folder, its README included, raises the version; stamped by the `nerdbank-gitversioning` npm package, whose `nbgv-setversion` writes the version into `package.json` at release over the committed placeholder; released by `typescript-release-daxformatter-mcp.yml` to npm, with provenance.
- Both release workflows read the version from the same NBGV copy that stamps the package: two NBGV versions can compute different heights for the same commit, so another copy could tag or push a version other than the packed one. They run only from `master` and create a draft GitHub release.
- `dotnet-ci.yml` and `typescript-ci.yml` run on pull requests and pushes to `master` that change their language folder or their own file, and by hand. Their job ids start with the language, so each check, named after its job, is unique across workflows.
- A required check that a path filter skips stays pending and blocks merging ([Troubleshooting required status checks](https://docs.github.com/en/pull-requests/how-tos/merge-and-close-pull-requests/troubleshooting-required-status-checks)), so neither CI check can be required: a required check needs a job that runs on every pull request, such as a gate job in a workflow without path filters.
