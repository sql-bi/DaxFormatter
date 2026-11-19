# AGENTS.md

The .NET client library `Dax.Formatter`, published on NuGet, with its tests and a getting-started sample. This file adds the rules of `dotnet/` to the root [`AGENTS.md`](../AGENTS.md).

## Commands

Run from `dotnet/`, never from the repository root ([why](../CONTRIBUTING.md#net)).

```bash
dotnet build ./Dax.Formatter.sln --configuration Release
dotnet test ./tests/Dax.Formatter.Tests/Dax.Formatter.Tests.csproj --configuration Release
dotnet pack ./src/Dax.Formatter/Dax.Formatter.csproj --configuration Release
# the sample, against the package on nuget.org and without the build settings of dotnet/
dotnet build ./samples/GettingStarted/getting-started.sln
```

## Done means

- Build, tests and pack succeed from `dotnet/`, with no new warnings.
- The tests call the live DAX Formatter service: they need network access, and a failure can come from the service rather than from the change.

## Boundaries

- Ask first: changes to `Directory.Build.props`, package references, target frameworks, or the public API of `Dax.Formatter`, which is a published package.
- Never: edit `global.json` or `nuget.config` to make a build pass.
- Never: commit `src/Dax.Formatter/DaxFormatter.snk`, the strong-name key that the release workflow writes from a secret; `.gitignore` ignores it.
- Never: delete `samples/Directory.Build.props` or `samples/Directory.Build.targets`. They contain only `<Project />`, so the samples don't inherit the build settings of `dotnet/`; an empty file fails with MSB4024.
- Never: run `dotnet format`, lower `AnalysisMode` or `AnalysisLevel`, or add a broad `NoWarn` to get a clean build.
- Always: follow the root `.editorconfig` (C# style, indentation, encoding) in the lines you change; don't reformat the rest of a file.

## When you change X, update Y

- `version.json` → read the [NBGV caveat](../docs/design/architecture.md#versions-and-releases) first; after the change, check with the version command of the [release guide](../docs/guides/release.md#before-you-start) that a commit under `src/Dax.Formatter` still raises the version.
- The public API or the behavior of the client → `README.md` in this folder, the NuGet package readme, written for package users and packed into the package.
- A new project → `src/`, `tests/` or `samples/` ([ADR-0001](../docs/decisions/0001-organize-the-repository-by-language.md), [ADR-0003](../docs/decisions/0003-keep-one-net-solution-with-role-folders.md)); libraries and tests go into `Dax.Formatter.sln`. A new test project also goes into the test step of `dotnet-ci.yml` and of the release workflow, which name the test project they run.

## Conventions

- A new test that doesn't need the service doesn't call it.
