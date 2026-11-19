# AGENTS.md

The .NET client library `Dax.Formatter`, published on NuGet, with its tests and a getting-started sample. This file adds the rules of `dotnet/` to the root [`AGENTS.md`](../AGENTS.md).

## Commands

Run from `dotnet/`, never from the repository root: the `dotnet` CLI looks for `global.json` from the current folder upward, so only there does it use the SDK that `dotnet/global.json` pins.

```bash
dotnet build ./Dax.Formatter.sln --configuration Release
dotnet test ./tests/Dax.Formatter.Tests/Dax.Formatter.Tests.csproj --configuration Release
dotnet pack ./src/Dax.Formatter/Dax.Formatter.csproj --configuration Release
# the version a release would publish, when run on an up-to-date master; elsewhere it has a commit suffix
dotnet msbuild ./src/Dax.Formatter/Dax.Formatter.csproj -restore -target:GetBuildVersion -getProperty:NuGetPackageVersion
# the sample, against the package on nuget.org and without the build settings of dotnet/
dotnet build ./samples/GettingStarted/getting-started.sln
```

- The test project also targets `net472`, which runs only on Windows; elsewhere, add `--framework net10.0` to `dotnet test`.
- Local builds are not strong-named: the build signs only when `src/Dax.Formatter/DaxFormatter.snk` exists (`Directory.Build.props`), and only `dotnet-release-dax-formatter.yml` writes it, from the `SIGNING_KEY` secret.

## Done means

- Build, tests and pack succeed from `dotnet/`, with no new warnings.
- The tests call the live DAX Formatter service: they need network access, and a failure can come from the service rather than from the change.

## Boundaries

- Ask first: changes to `Directory.Build.props`, package references, target frameworks, or the public API of `Dax.Formatter`, which is a published package.
- Never: edit `global.json` or `nuget.config` to make a build pass.
- Never: commit the strong-name key `src/Dax.Formatter/DaxFormatter.snk`; `.gitignore` ignores it.
- Never: delete `samples/Directory.Build.props` or `samples/Directory.Build.targets`. They contain only `<Project />`, so the samples don't inherit the build settings of `dotnet/`; an empty file fails with MSB4024.
- Never: run `dotnet format`, lower `AnalysisMode` or `AnalysisLevel`, or add a broad `NoWarn` to get a clean build.
- Always: follow the root `.editorconfig` (C# style, indentation, encoding) in the lines you change; don't reformat the rest of a file.

## When you change X, update Y

- `version.json` → read the [pitfall](#pitfalls) below first; after the change, check with the version command above that a commit under `src/Dax.Formatter` still raises the version.
- The `.snk` path in `Directory.Build.props` → the path where `dotnet-release-dax-formatter.yml` writes the key, and the other way round: a mismatch builds an unsigned package without an error.
- The public API or the behavior of the client → `README.md` in this folder, the NuGet package readme, written for package users and packed into the package.
- A new project → `src/`, `tests/` or `samples/` ([ADR-0001](../docs/decisions/0001-organize-the-repository-by-language.md), [ADR-0003](../docs/decisions/0003-keep-one-net-solution-with-role-folders.md)); libraries and tests go into `Dax.Formatter.sln`. A new test project also goes into the test step of `dotnet-ci.yml` and of the release workflow, which name the test project they run.

## Pitfalls

- `version.json` must never have the same content as `git show 6df1bf0^:version.json`, the root `version.json` before the move to `dotnet/`, whose path filter is `src/Dax.Formatter`. While the two are identical, NBGV resolves the path filter from the old root: commits under `dotnet/src/Dax.Formatter` don't raise the version, and the computed version is one already published on nuget.org. The two differ only in how the filter is written, so it stays `./src/Dax.Formatter`.

## Conventions

- A new test that doesn't need the service doesn't call it.
