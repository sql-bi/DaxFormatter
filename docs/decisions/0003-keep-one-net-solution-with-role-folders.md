---
status: accepted
date: 2026-11-19
decision-makers: Alberto Spelta, Marco Russo
---

# Keep one .NET solution with role folders

## Context and Problem Statement

[ADR-0001](0001-organize-the-repository-by-language.md) makes `dotnet/` the .NET toolchain root, with projects under `src/`, `tests/` and `samples/`, but it doesn't weigh that layout against others. Today `dotnet/` holds one NuGet package, `Dax.Formatter`: the library in `src/Dax.Formatter`, its tests in `tests/Dax.Formatter.Tests`, both in `Dax.Formatter.sln`, and a getting-started sample in `samples/GettingStarted` with a solution of its own, which builds against the published package. A second NuGet package would have to fit in. Should `dotnet/` keep one solution with folders by role, or give each package a folder with its own source, tests and samples? And where would a second package's NBGV `version.json` go, since [ADR-0004](0004-version-tag-and-release-each-package-independently.md) gives each package its own version?

## Considered Options

* One solution with role folders: `src/<Project>`, `tests/<Project>.Tests`, `samples/<Sample>`
* One folder per package: `dotnet/<Package>/` with its own `src/`, `tests/` and `samples/`
* Projects side by side in one folder, as before the move: the library and its tests next to each other

## Decision Outcome

Chosen option: "One solution with role folders", because one package, its tests and any later package share one solution, one set of build settings and one CI workflow, which runs on any change under `dotnet/` and builds that solution ([ADR-0001](0001-organize-the-repository-by-language.md)); it is the layout of the multi-package .NET repositories checked under More Information that keep one solution; and it is the layout that the move of ADR-0001 already built.

- `Dax.Formatter.sln` holds the libraries under `src/` and the test projects under `tests/`; `Directory.Build.props` in `dotnet/` applies to both.
- Samples under `samples/` show how users consume a published package: each has its own solution, references the package from nuget.org, and inherits no build settings from `dotnet/`.
- A second NuGet package adds its project under `src/` and its tests under `tests/`, in the same solution, and a release workflow of its own, `dotnet-release-<package>.yml` ([ADR-0001](0001-organize-the-repository-by-language.md)).
- A second NuGet package puts its own `version.json` in its project folder, `src/<Package>/version.json`, with path filters that cover that folder and whatever it builds from. NBGV uses the `version.json` in the project's folder or its nearest ancestor, and its documentation shows a secondary `version.json` in a project folder ([`version.json`](https://dotnet.github.io/Nerdbank.GitVersioning/docs/versionJson.html)). `dotnet/version.json` stays the version file of `Dax.Formatter`: its path filter names `src/Dax.Formatter`, and moving it has an NBGV caveat ([`dotnet/AGENTS.md`](../../dotnet/AGENTS.md#pitfalls)). Checked on a scratch repository with the nbgv CLI 3.7.112: a project under a folder with its own `version.json` gets that file's version and height, and commits there don't raise the version of a project under `dotnet/version.json`.

### Consequences

* Good, because `dotnet build`, `dotnet test` and `dotnet pack` from `dotnet/` cover every .NET package, and CI builds the same solution; only its test and pack steps, which name their projects, change for a new package.
* Good, because shared build settings stay in one `Directory.Build.props`, and the solution shows every library and test project together.
* Bad, because a second package's `version.json` sits in its project folder, not at the toolchain root: the rule of ADR-0004 that "each package has one NBGV `version.json` at its toolchain root" then holds for `Dax.Formatter` only, so a second NuGet package needs a record that supersedes that rule.
* Bad, because a change to the shared build settings rebuilds and retests every package, and path filters must list the shared files each package builds from if such changes are to raise its version.
* Neutral, because the precedents with role folders share one version prefix across their packages: independent versions inside role folders rest on NBGV alone.

### Confirmation

- Review: a new .NET library goes under `src/`, its tests under `tests/`, both in `Dax.Formatter.sln`; a sample goes under `samples/` with its own solution.
- A second NuGet package has its own `version.json` in its project folder, and `nbgv get-version -p dotnet/src/<Package>` gives its version, while `nbgv get-version -p dotnet` still gives the `Dax.Formatter` one.
- CI: `dotnet-ci.yml` builds `Dax.Formatter.sln` from `dotnet/`, so a project left out of the solution is not built.

## Pros and Cons of the Options

### One solution with role folders: `src/<Project>`, `tests/<Project>.Tests`, `samples/<Sample>`

* Good, because one solution, one `Directory.Build.props` and one CI workflow cover every package.
* Good, because the role of each project shows in its path, so a glob such as `src/**` names the shipped code and `tests/**` the tests.
* Good, because it is what `dotnet/` already is: nothing moves.
* Bad, because a package's code, tests and samples are in three places.
* Bad, because a second package's `version.json` can't sit at the toolchain root next to the first one's.

### One folder per package: `dotnet/<Package>/` with its own `src/`, `tests/` and `samples/`

* Good, because everything about a package is in one folder, and its `version.json` sits at the package's folder, close to ADR-0004's wording.
* Good, because it is the layout of Azure/azure-sdk-for-net, which versions each package on its own (`sdk/<service>/<package>/`).
* Bad, because `global.json`, `nuget.config` and the shared build settings stay in `dotnet/`, so a package folder is not a toolchain root either.
* Bad, because one package gains a level of folders and a solution, or the repository needs one solution per package plus one for all of them.
* Bad, because it moves files again, right after the move of ADR-0001, with the same costs: broken links to paths on `master` and history that needs `git log --follow`.

### Projects side by side in one folder, as before the move: the library and its tests next to each other

* Good, because paths are one level shorter; it is the layout of the .NET unit testing tutorials, with each project in a folder next to the solution.
* Bad, because shipped code and tests share one folder, so a glob or a path filter can't tell them apart by path.
* Bad, because it moves files again, as the option above.

## More Information

- The layout this record refines: [ADR-0001](0001-organize-the-repository-by-language.md); versions and tags per package: [ADR-0004](0004-version-tag-and-release-each-package-independently.md).
- No Microsoft documentation prescribes a folder layout for a .NET repository. The closest, the tutorial [Organizing and testing projects with the .NET CLI](https://learn.microsoft.com/dotnet/core/tutorials/testing-with-cli), puts the application under `src/` and the tests under `test/`, and notes that some developers keep them in the same folder.
- Precedents, checked on 2026-11-17 with the GitHub contents API:
  - [microsoft/agent-framework](https://github.com/microsoft/agent-framework), `dotnet/`: `src/`, `tests/`, `samples/`, one solution `agent-framework-dotnet.slnx`, a shared `VersionPrefix` in `nuget/nuget-package.props`.
  - [microsoft/semantic-kernel](https://github.com/microsoft/semantic-kernel), `dotnet/`: `src/`, `test/`, `samples/`, one solution `SK-dotnet.slnx`, a shared `VersionPrefix` in `nuget/nuget-package.props`.
  - [modelcontextprotocol/csharp-sdk](https://github.com/modelcontextprotocol/csharp-sdk): `src/` with several packages (`ModelContextProtocol`, `ModelContextProtocol.Core`, `ModelContextProtocol.AspNetCore` and others), `tests/`, `samples/`, one solution `ModelContextProtocol.slnx`, a shared `VersionPrefix` in `src/Directory.Build.props`.
  - [Azure/azure-sdk-for-net](https://github.com/Azure/azure-sdk-for-net), `sdk/<service>/<package>/` with `src/`, `tests/` and `samples/` per package, for example `sdk/storage/Azure.Storage.Blobs/`, a `<Version>` in each package's project file, and solutions per service such as `sdk/storage/Azure.Storage.sln`.
- NBGV: [`version.json`](https://dotnet.github.io/Nerdbank.GitVersioning/docs/versionJson.html), including secondary files in project folders, and [path filters](https://dotnet.github.io/Nerdbank.GitVersioning/docs/path-filters.html), relative to the `version.json` that declares them.
- Revisit when a second NuGet package is planned, together with the rule of ADR-0004 on where its `version.json` goes, or if the packages start to need separate build settings.
