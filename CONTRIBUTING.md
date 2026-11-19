# Contributing

Thank you for helping with DAX Formatter. This repository holds the client packages; the DAX Formatter service itself is maintained elsewhere, so a problem with a formatting result is reported here as an issue, not fixed with a pull request.

## Issues

- Report a bug or request a feature in [GitHub Issues](https://github.com/sql-bi/DaxFormatter/issues), with the form that fits.
- Report a security vulnerability privately, as the [security policy](SECURITY.md) explains, never in a public issue.
- For a change larger than a fix, open an issue first, so that the approach is agreed before the work starts.

## Where code goes

The repository is organized by language ([ADR-0001](docs/decisions/0001-organize-the-repository-by-language.md)):

- `dotnet/`: the .NET client library `Dax.Formatter`, its tests and samples ([ADR-0003](docs/decisions/0003-keep-one-net-solution-with-role-folders.md)).
- `typescript/packages/daxformatter-mcp/`: the MCP server `@sqlbi/daxformatter-mcp`, with its own TypeScript port of the client.
- `docs/`: documentation for contributors and maintainers; [`docs/README.md`](docs/README.md) says where each kind of fact lives. The user documentation is on [SQLBI Docs](https://docs.sqlbi.com/dax-formatter/).

## Build and test

### .NET

Run the commands from `dotnet/`: the `dotnet` CLI looks for `global.json` from the current folder upward, so only there does it use the SDK that `dotnet/global.json` pins.

```bash
cd dotnet
dotnet build
dotnet test
dotnet pack
```

- The tests call the live DAX Formatter service: they need network access, and they fail when the service is unreachable.
- The test project also targets .NET Framework 4.7.2, which runs only on Windows, where CI runs; on other systems, run `dotnet test --framework net10.0`.
- Local builds are not strong-named: the signing key exists only in the release workflow.

### TypeScript

Run the commands from the package folder; each npm package has its own lockfile.

```bash
cd typescript/packages/daxformatter-mcp
npm ci
npm run typecheck
npm run build
npm test
```

The tests replace `fetch` with a mock and run offline. Build and test with a current release of the Node.js major version that `.github/workflows/typescript-ci.yml` sets: the build and test tools need a newer Node.js than the published package, which supports the versions in the `engines` field of `package.json`.

## Commits and pull requests

- Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/): `type(scope): summary`, in the imperative, lowercase start, at most 72 characters, for example `docs(mcp): link client setup on SQLBI Docs`. Usual types: `feat`, `fix`, `docs`, `refactor`, `test`, `build` (also dependencies, as Dependabot writes them), `ci`, `chore`.
- One logical change per commit; the body says what changed and why, in `-` bullets wrapped at 72 columns.
- Pull requests are squash-merged into `master`, so the pull request title becomes the commit subject: write it as a Conventional Commits subject. The description is plain text with `-` bullets, without headings or tables.
- CI builds and tests the language folders that a pull request changes, and again after the merge into `master`.
- A change updates the documents it affects in the same pull request; a change to the REST contract starts from [`docs/design/rest-api.md`](docs/design/rest-api.md) and updates both clients. An architectural decision is recorded in [`docs/decisions/`](docs/decisions/README.md).

## Versions, tags and releases

- Each package gets its version from Nerdbank.GitVersioning and its own `version.json` ([ADR-0004](docs/decisions/0004-version-tag-and-release-each-package-independently.md)): don't set versions in project files or in `package.json`.
- The maintainers release each package with its release workflow; the [release guide](docs/guides/release.md) gives the steps and the tags.

## Reading history across the move

On 2026-11-17 the repository moved to the language-first layout in one commit, `6df1bf0` (#53). To follow a file across the move, use `git log --follow`:

```bash
git log --follow -- dotnet/src/Dax.Formatter/DaxFormatterClient.cs
```

Commits before the move use the old paths:

| Before the move | After the move |
|---|---|
| `src/Dax.Formatter/` | `dotnet/src/Dax.Formatter/` |
| `src/Dax.Formatter.Tests/` | `dotnet/tests/Dax.Formatter.Tests/` |
| `src/Dax.Formatter.sln`, `src/Directory.Build.props` | `dotnet/` |
| `samples/getting-started/` | `dotnet/samples/GettingStarted/` |
| `version.json`, `global.json`, `nuget.config`, `package-icon.png`, `README.md` at the root | `dotnet/` |
| `mcp/` | `typescript/packages/daxformatter-mcp/` |
| `.github/workflows/ci.yml`, `release.yml` | `dotnet-ci.yml`, `dotnet-release-dax-formatter.yml` |
| `.github/workflows/ci-mcp.yml`, `publish-mcp.yml` | `typescript-ci.yml`, `typescript-release-daxformatter-mcp.yml` |

For a folder, or for commands that don't follow renames, give both paths, for example `git log -- src/Dax.Formatter dotnet/src/Dax.Formatter`.

## License

The repository is licensed under the [MIT license](LICENSE.md).
