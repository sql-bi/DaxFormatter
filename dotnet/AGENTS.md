# AGENTS.md

The .NET client library `Dax.Formatter`, published on NuGet, with its tests. This file adds the rules of `dotnet/` to the root [`AGENTS.md`](../AGENTS.md).

## Commands

Run from `dotnet/`, never from the repository root: the `dotnet` CLI looks for `global.json` from the current folder upward, so only there does it use the SDK that `dotnet/global.json` pins.

```bash
dotnet build ./Dax.Formatter.sln --configuration Release
dotnet test ./tests/Dax.Formatter.Tests/Dax.Formatter.Tests.csproj --configuration Release
dotnet pack ./src/Dax.Formatter/Dax.Formatter.csproj --configuration Release
# the version a release would publish, when run on an up-to-date master; elsewhere it has a commit suffix
dotnet msbuild ./src/Dax.Formatter/Dax.Formatter.csproj -restore -target:GetBuildVersion -getProperty:NuGetPackageVersion
```

- The test project also targets `net472`, which runs only on Windows; elsewhere, add `--framework net10.0` to `dotnet test`.
- The tests run offline. Those in `tests/Dax.Formatter.Tests/Live/` call the live DAX Formatter service and are skipped unless the environment variable `DAXFORMATTER_LIVE_TESTS` is set; add `--filter "Category=Live"` to run only them. Run them when a change touches what the client sends or how it reads an answer: they send a few tens of requests, with made-up DAX only.
- Local builds are not strong-named: the build signs only when `src/Dax.Formatter/DaxFormatter.snk` exists (`Directory.Build.props`), and only `dotnet-release-dax-formatter.yml` writes it, from the `SIGNING_KEY` secret.

## Done means

- Build, tests and pack succeed from `dotnet/`, with no warnings: the build of `master` has none.
- A live test that fails while the offline tests pass means that the service changed: say so, don't change the fixture to make it pass without checking the service.

## Boundaries

- Ask first: changes to `Directory.Build.props`, package references, target frameworks, or the public API of `Dax.Formatter`, which is a published package. `PublicApiTests` fails when the public API changes: update `tests/Dax.Formatter.Tests/PublicApi.approved.txt` only after the change is approved.
- Never: edit `global.json` or `nuget.config` to make a build pass.
- Never: commit the strong-name key `src/Dax.Formatter/DaxFormatter.snk`; `.gitignore` ignores it.
- Never: run `dotnet format`, lower `AnalysisMode` or `AnalysisLevel`, or add a broad `NoWarn` to get a clean build.
- Never: call the network from a test outside `Live/`: use `FormatterHarness` and the responders of `Respond`, as the existing tests do.
- Always: follow the root `.editorconfig` (C# style, indentation, encoding) in the lines you change; don't reformat the rest of a file.

## When you change X, update Y

- `version.json` → the version rules of [ADR-0006](../docs/decisions/0006-write-the-dax-formatter-package-version-by-hand.md), in short:
  - `master` holds the next version to publish, three numbers written by hand;
  - right after a release, a one-line pull request raises the patch number (`2.0.0` → `2.0.1`);
  - a pull request that adds a feature sets the minor number (`2.1.0`), one with a breaking change the major number;
  - a preview is written out: `2.0.0-beta.1`, `2.0.0-beta.2`, then `2.0.0`.
- The `.snk` path in `Directory.Build.props` → the path where `dotnet-release-dax-formatter.yml` writes the key, and the other way round: a mismatch builds an unsigned package without an error.
- The public API or the behavior of the client → [`docs/design/client-api.md`](../docs/design/client-api.md), which the clients of other languages port, and `README.md` in this folder, the NuGet package readme, written for package users and packed into the package.
- A C# code block of `README.md` → the region of the same name in `tests/Dax.Formatter.Tests/Readme/ReadmeSamples.cs`, and the other way round: `ReadmeTests` fails when a block and its region differ, and the test around the region runs it. The examples of the package live only there: there is no sample project.
- What the client sends or expects from the service → the fixtures in `tests/Dax.Formatter.Tests/Fixtures/`, with answers recorded from the service, never written by hand; then run the live tests.
- A new project → `src/`, `tests/` or `samples/` ([ADR-0001](../docs/decisions/0001-organize-the-repository-by-language.md), [ADR-0003](../docs/decisions/0003-keep-one-net-solution-with-role-folders.md)); libraries and tests go into `Dax.Formatter.sln`. A new test project also goes into the test step of `dotnet-ci.yml` and of the release workflow, which name the test project they run.

## Pitfalls

- A commit under `src/Dax.Formatter` no longer raises the version of the package: it raises only the fourth number of the file version. The check after a change of `version.json` is that, on an up-to-date `master`, the version command above prints the version written in `version.json`.
- `version.json` must never have the same content as `git show 6df1bf0^:version.json`, the root `version.json` before the move to `dotnet/`, whose version is `1.2` and whose path filter is `src/Dax.Formatter`. While the two are identical, NBGV resolves the path filter from the old root and computes the height from there. The path filter stays `./src/Dax.Formatter`.
- The library builds for `netstandard2.0` and `net10.0`, and the tests run it on .NET Framework too. An API that only .NET 10 has needs an `#if NET` branch or a polyfill in `src/Dax.Formatter/Polyfills/`, and the analyzers of one target can ask for what the other lacks: build both before trusting a fix.
- In `ReadmeSamples.cs` the name `DaxFormatterClient` is an alias of `ReadmeClient`, declared inside the namespace. Outside the namespace the alias loses to the real type, and the samples would call the live service.

## Conventions

- C# follows the root `.editorconfig` and the analyzers at `AnalysisMode=all`: fix a warning, don't suppress it. The one suppression is `CA2007` in `ReadmeSamples.cs`, whose regions are the code of the readme.
- Internal types are `internal sealed`, in the folder of their responsibility ([`client-api.md`](../docs/design/client-api.md#code-and-tests)); public types are in the root folder and in the `Dax.Formatter` namespace.
- One test class for each production class, at the same path; test names are `Method_Scenario_Expected`. In a test, `ConfigureAwait(true)` satisfies both CA2007 and xUnit1030.
- The test packages stay as they are: xUnit 2 with fakes written by hand, no mocking library.
