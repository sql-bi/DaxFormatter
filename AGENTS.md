# AGENTS.md

Client packages for the DAX Formatter service, organized by language: the .NET client library `Dax.Formatter` (NuGet) in `dotnet/`, and the MCP server `@sqlbi/daxformatter-mcp` (npm), which carries its own TypeScript port of the client, in `typescript/packages/daxformatter-mcp/`. The service, its REST API and its remote MCP server are maintained outside this repository.

## Commands

There is no build at the root: each language folder builds and tests on its own, with the commands and rules in [`dotnet/AGENTS.md`](dotnet/AGENTS.md) and [`typescript/AGENTS.md`](typescript/AGENTS.md).

## Done means

- The build and the tests of every language folder you changed pass.
- Every relative link in the Markdown files you changed resolves.
- Each `AGENTS.md` stays under 200 lines, and each `CLAUDE.md` contains only `@AGENTS.md`.
- The documents affected by the change are updated (see below).

## Boundaries

- Always: work on a branch, never on `master`; one logical change per commit; keep each fact in one home ([Documentation](#documentation)) and link to it.
- Ask first: changes to `.github/workflows/`, `.github/dependabot.yml` or `.github/CODEOWNERS`; any `version.json`; package ids, tags or release conventions; the wire format of a request or response; a new dependency.
- Never: commit secrets, tokens or signing keys; rewrite published history (force-push `master`, move or delete tags); run a release workflow without being asked.
- Never: add `docs/plans/`; the state of work in progress stays outside this repository.
- Never, because this repository is public: name private repositories, internal hosts, cloud resources, private package feeds or secret values in any file, commit message or pull request.

## When you change X, update Y

- The REST contract (an endpoint, a field, an enum value, the hashing, the caller identity) → [`docs/design/rest-api.md`](docs/design/rest-api.md) and every implementation its [Implementations](docs/design/rest-api.md#implementations) section lists.
- A workflow file renamed or added → its file name follows [ADR-0001](docs/decisions/0001-organize-the-repository-by-language.md) (`<language>-ci.yml`, `<language>-release-<package>.yml`), its `name:` is the file name without `.yml`, and its CI job ids start with the language, such as `dotnet-build-and-test` ([why](docs/design/architecture.md#deployment)); for the .NET CI workflow, the CI badge in `dotnet/README.md`; for the .NET release workflow, a maintainer adds the nuget.org Trusted Publishing policy for the new file name before the next release.
- A new package → its language folder ([ADR-0001](docs/decisions/0001-organize-the-repository-by-language.md)); its own `version.json` at its toolchain root ([ADR-0004](docs/decisions/0004-version-tag-and-release-each-package-independently.md)), or, for a second NuGet package, in its project folder ([ADR-0003](docs/decisions/0003-keep-one-net-solution-with-role-folders.md)) with a record that supersedes that rule of ADR-0004; its release workflow `<language>-release-<package>.yml`; the command that prints its version in its language folder's `AGENTS.md`; an entry in `.github/dependabot.yml` when no entry covers its folder; the component dropdown of the issue forms; the package table of `README.md`; [`docs/design/architecture.md`](docs/design/architecture.md).
- How the packages fit together, version or release → [`docs/design/architecture.md`](docs/design/architecture.md).
- An architectural decision → a new record in [`docs/decisions/`](docs/decisions/README.md), in the same pull request.
- How to connect an AI client to the MCP servers → SQLBI Docs ([remote](https://docs.sqlbi.com/dax-formatter/mcp-http), [stdio](https://docs.sqlbi.com/dax-formatter/mcp-stdio)), maintained outside this repository, which only links them.
- Work planned, open decisions, known debt → [`docs/backlog.md`](docs/backlog.md).

## Conventions

- Accepted records in [`docs/decisions/`](docs/decisions/README.md) are binding: when a change contradicts one, stop and ask. Proposed records are context.
- Commits and pull requests follow [`CONTRIBUTING.md`](CONTRIBUTING.md#commits-and-pull-requests).
- Workflows follow GitHub's [secure use reference](https://docs.github.com/en/actions/reference/security/secure-use): actions pinned to a full commit SHA with the version in a comment on the same line, which [Dependabot updates](https://docs.github.com/en/code-security/dependabot/ecosystems-supported-by-dependabot/supported-ecosystems-and-repositories#github-actions) with the SHA; `permissions: contents: read` at the top, and write permissions only on the job that needs them; expressions passed to scripts through `env`, never written into `run`; `persist-credentials: false` on every checkout.
- Documents are in plain English, describe the present, and link within the repository with relative links. Package READMEs are written for their registry, with only what a package user needs: the npm README uses absolute URLs only.

## Documentation

- [`docs/design/`](docs/design/README.md): how the packages work, call the service, version and release. Read before changing a package's boundaries, the REST contract, a `version.json` or a workflow.
- [`docs/decisions/`](docs/decisions/README.md): why it is so, one record per decision. Read the titles before proposing a design choice.
- [`docs/backlog.md`](docs/backlog.md): what comes next, open decisions and known debt. Read before picking up work; a new item takes the `Next ID`, and IDs are never reused.
