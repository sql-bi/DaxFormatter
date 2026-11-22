---
status: accepted
date: 2026-11-22
decision-makers: Alberto Spelta
---

# Write the Dax.Formatter package version by hand

## Context and Problem Statement

`dotnet/version.json` held the two-part version `1.2`, and Nerdbank.GitVersioning (NBGV) added the git height as the third number: every commit under `dotnet/src/Dax.Formatter` raised the version that the next release would publish, so 1.2.2 came after 1.2.1 whatever the commits changed ([ADR-0004](0004-version-tag-and-release-each-package-independently.md)). The number says how many commits a release holds, not whether an application can take it without changes. Version 2.0.0 is a breaking release ([ADR-0005](0005-decouple-the-client-api-from-the-rest-contract.md)) that may be published as a preview first, and after it a fix, a feature and a breaking change should each be recognizable from the number. Who decides the version of the `Dax.Formatter` package, and where is it written?

## Considered Options

* Keep the two-part version, with the git height as the patch number
* Write a three-part version by hand in `version.json`, and keep NBGV to stamp it
* Remove NBGV from the .NET toolchain and set the version in the project file

## Decision Outcome

Chosen option: "Write a three-part version by hand in `version.json`, and keep NBGV to stamp it", because the number then says what a release changes, as Semantic Versioning asks, while the build, the release workflow and the tags stay as they are.

- `dotnet/version.json` holds a three-part version, such as `2.0.0`, or a preview such as `2.0.0-beta.1`. NBGV stamps it on the package and the assemblies. The git height becomes the fourth number of the file version, and appears nowhere in the package version; the path filter decides only that number.
- `master` holds the next version to publish.
- Right after a release, a one-line pull request raises the patch number: `2.0.0` becomes `2.0.1`.
- A pull request that adds a feature sets the minor number, `2.1.0`; one with a breaking change sets the major number.
- Previews are written out: `2.0.0-beta.1`, `2.0.0-beta.2`, then `2.0.0`.
- This holds for `Dax.Formatter`. The MCP server stays on `0.2-beta` with the height as its third number while it is in beta.
- The assembly version keeps only the major number, and a build outside `master` keeps its commit suffix: both settings of `version.json` are unchanged.

### Consequences

* Good, because a version says what it changes: a patch is a fix, a minor adds, a major breaks.
* Good, because a preview can be published and replaced by the final version with the same number.
* Good, because the file version still tells two builds of the same package version apart.
* Bad, because the version is now a manual step: a release from a `master` whose version was already published is stopped only by `dotnet nuget push`, after the build and the tests, as ADR-0004 already notes. The [backlog](../backlog.md) has an item for an earlier check.
* Bad, because a pull request that adds a feature and doesn't set the minor number publishes it as a patch: only a review catches it.
* Bad, because the two packages of the repository follow different schemes until the MCP server leaves its beta.

### Confirmation

- On an up-to-date `master`, the version command of [`dotnet/AGENTS.md`](../../dotnet/AGENTS.md) prints the version written in `dotnet/version.json`.
- Review: a pull request that changes the public API or the behavior of `Dax.Formatter` sets the version by the rules above, and the pull request after a release raises the patch number.
- Checked when this record was written, with NBGV 3.9.50: two commits in a row that change `dotnet/src/Dax.Formatter` give the same package version and file versions whose fourth numbers differ by one.

## Pros and Cons of the Options

### Keep the two-part version, with the git height as the patch number

* Good, because no one sets a version, so no release can reuse one.
* Bad, because the patch number counts commits: a release with a new feature and one with a typo fix look the same.
* Bad, because a preview of 2.0 would need a `-beta` tag on every build until the final version, with the height in the number.

### Write a three-part version by hand in `version.json`, and keep NBGV to stamp it

* Good, because the build, the release workflow, the tags and the commit suffix outside `master` don't change.
* Good, because NBGV documents this form: with a third number in the version, the height is the fourth one.
* Bad, because the version must be raised by hand, in the right pull request.

### Remove NBGV from the .NET toolchain and set the version in the project file

* Good, because one tool less, and the version sits next to the other package properties.
* Bad, because the release workflow reads the version from NBGV, and a build outside `master` would lose its commit suffix, so a local package could pass for a released one.
* Bad, because it changes `Directory.Build.props` and a release workflow for a gain that the chosen option already gives. The backlog keeps it as an open decision.

## More Information

- This record replaces one rule of [ADR-0004](0004-version-tag-and-release-each-package-independently.md) for `Dax.Formatter`: "Each package has one NBGV `version.json` at its toolchain root, with path filters, so that only changes to the package raise its version height". The file and its path filter stay, but a change to the package no longer raises its version. The other rules of ADR-0004 stand: one `version.json` for each package, the tags, the draft releases.
- NBGV, [`version.json`](https://dotnet.github.io/Nerdbank.GitVersioning/docs/versionJson.html): "You may optionally supply a third integer in the version (i.e. x.y.z), in which case the git version height is specified as the fourth integer, which only appears in certain version representations."
- Microsoft, [Versioning and .NET libraries](https://learn.microsoft.com/dotnet/standard/library-guidance/versioning): consider Semantic Versioning 2.0.0 for the package version, include a prerelease suffix when releasing a nonstable package, consider only a major version in the assembly version, and consider a continuous integration build number as the revision of the file version.
- Azure SDK, [release policy](https://azure.github.io/azure-sdk/policies_releases.html#incrementing-after-release): the version in source control is raised right after a package ships, because "It's safer to have `N+1` in `main` than `N`"; previews are `X.Y.Z-beta.N`. Its .NET packages go from a stable version to the first beta of the next minor one; here the smaller step, the next patch, is the default, and a pull request with a feature raises it.
- [Semantic Versioning 2.0.0](https://semver.org/spec/v2.0.0.html).
- Revisit when the MCP server leaves its beta, to give both packages one scheme, or if NBGV is removed from the .NET toolchain.
