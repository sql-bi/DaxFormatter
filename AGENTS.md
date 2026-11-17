# AGENTS.md

Client packages for the DAX Formatter service, organized by language: the .NET client library `Dax.Formatter` (NuGet) in `dotnet/`, and the MCP server `@sqlbi/daxformatter-mcp` (npm), which carries its own TypeScript port of the client, in `typescript/packages/daxformatter-mcp/`. The service, its REST API and its remote MCP server are maintained outside this repository. Documentation map: [`docs/README.md`](docs/README.md).

## Commands

There is no build at the root: each language builds and tests from its own folder, with the commands and rules in [`dotnet/AGENTS.md`](dotnet/AGENTS.md) and [`typescript/AGENTS.md`](typescript/AGENTS.md).

## Done means

- The build and the tests of every language folder you changed pass.
- Every relative link in the Markdown files you changed resolves.
- Each `AGENTS.md` stays under 200 lines, and each `CLAUDE.md` contains only `@AGENTS.md`.
- The documents affected by the change are updated (see below).

## Boundaries

- Always: work on a branch, never on `master`; one logical change per commit; keep each fact in one home and link to it ([`docs/README.md`](docs/README.md)).
- Ask first: changes to `.github/workflows/`, `.github/dependabot.yml` or `.github/CODEOWNERS`; any `version.json`; package ids, tags or release conventions; the wire format of a request or response; a new dependency.
- Never: commit secrets, tokens or signing keys; rewrite published history (force-push `master`, move or delete tags); run a release workflow without being asked.
- Never, because this repository is public: name private repositories, internal hosts, cloud resources, private package feeds or secret values in any file, commit message or pull request.

## When you change X, update Y

- The REST contract (an endpoint, a field, an enum value, the hashing, the caller identity) → [`docs/design/rest-api.md`](docs/design/rest-api.md) and every implementation its [Implementations](docs/design/rest-api.md#implementations) section lists.
- A workflow file renamed or added → follow the naming rule of [ADR-0001](docs/decisions/0001-organize-the-repository-by-language.md) (`<language>-ci.yml`, `<language>-release-<package>.yml`), with a `name:` equal to the file name without `.yml`, and CI job ids prefixed with the language (`dotnet-build-and-test`), so that each pull request check, named after its job, is unique across workflows; update [`docs/guides/release.md`](docs/guides/release.md); for the .NET CI workflow, the CI badge in `dotnet/README.md`; for the .NET release workflow, a maintainer adds the nuget.org Trusted Publishing policy for the new file name before the next release.
- A new package → its language folder ([ADR-0001](docs/decisions/0001-organize-the-repository-by-language.md)); its own `version.json` ([ADR-0002](docs/decisions/0002-version-and-tag-each-package-independently.md); where a second NuGet package's goes is still open, see the proposed [ADR-0003](docs/decisions/0003-keep-one-net-solution-with-role-folders.md)); its release workflow `<language>-release-<package>.yml`, which leaves out the previous-tag lookup and `--notes-start-tag` until its first release is tagged; an entry in `.github/dependabot.yml` when no entry covers its folder; the component dropdown of the issue forms; the package table of the root `README.md`; [`docs/design/architecture.md`](docs/design/architecture.md); the release guide.
- How the packages fit together, version or release → [`docs/design/architecture.md`](docs/design/architecture.md); a release step → [`docs/guides/release.md`](docs/guides/release.md).
- An architectural decision → a new record in [`docs/decisions/`](docs/decisions/README.md), in the same pull request.
- How to connect an AI client to the MCP servers → SQLBI Docs ([remote](https://docs.sqlbi.com/dax-formatter/mcp-http), [stdio](https://docs.sqlbi.com/dax-formatter/mcp-stdio)), which are maintained outside this repository; this repository only links them.
- Work planned, open decisions, known debt → [`docs/backlog.md`](docs/backlog.md); a large piece of work in progress → its plan in `docs/plans/`, with its Progress updated in every pull request.

## Conventions

- Accepted records in [`docs/decisions/`](docs/decisions/README.md) are binding: when a change contradicts one, stop and ask. Proposed records are context.
- Commits and pull requests follow [`CONTRIBUTING.md`](CONTRIBUTING.md#commits-and-pull-requests).
- Documents are in plain English, describe the present, and link within the repository with relative links. Package READMEs are written for their registry, with only what a package user needs: the npm README uses absolute URLs only.
