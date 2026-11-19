---
status: superseded by ADR-0004
date: 2026-11-19
decision-makers: Alberto Spelta, Marco Russo
---

# Version and tag each package independently

## Context and Problem Statement

The two packages release on different channels and at different maturity: `Dax.Formatter` on NuGet in the stable 1.2 line, `@sqlbi/daxformatter-mcp` on npm in the 0.1 beta line. Each already gets its version from its own Nerdbank.GitVersioning (NBGV) `version.json` with path filters, but the tags follow two ad-hoc formats: `v<version>` for .NET (`v1.0.0-preview4` to `v1.2.2`, the format `release.yml` creates) and `DaxFormatter_MCP_v<version>` for the MCP server (`DaxFormatter_MCP_v0.1.3-beta` and `DaxFormatter_MCP_v0.1.4-beta`, created by `publish-mcp.yml`). More packages are planned ([ADR-0001](0001-organize-the-repository-by-language.md)). How should each package be versioned, tagged and released, so that a tag names its package, the "Latest" GitHub release stays the stable .NET client, and release notes start from the package's own previous release? The change is planned in [`language-first-layout.md`](../plans/language-first-layout.md).

## Considered Options

* One shared version and tag for all packages
* Independent versions, keeping today's ad-hoc tag names
* Independent versions with one `version.json` per package and `<published id>-v<version>` tags

## Decision Outcome

Chosen option: "Independent versions with one `version.json` per package and `<published id>-v<version>` tags", because the packages differ in channel and maturity, NBGV already computes one version per folder, and one tag pattern is predictable for people, scripts and a tag ruleset.

- Each package has one NBGV `version.json` at its toolchain root, with path filters so that only changes to the package raise its version height.
- Tags are `<published id>-v<version>`, where the id is the NuGet package id or the npm package name without its `@sqlbi/` scope: `Dax.Formatter-v1.2.3`, `daxformatter-mcp-v0.2.1-beta`.
- Existing tags stay as they are.
- A tag ruleset protects `*-v*` and `v*`.
- GitHub releases: the .NET client with `gh release create --latest`, the MCP server with `--prerelease --latest=false`, both with `--notes-start-tag` set to the package's previous tag. For the first release after this change, that is the package's last legacy tag (today `v1.2.2` and `DaxFormatter_MCP_v0.1.4-beta`).
- The MCP server's version goes to 0.2-beta with the move of ADR-0001: older commits have no `version.json` at the package's new path, so NBGV finds the root one, with a different version, and the height restarts, which on 0.1 would give 0.1.x-beta versions that collide with those already on npm (0.1.2-beta to 0.1.4-beta). The .NET client stays on 1.2: in older commits the nearest `version.json` is the root one with the same version, so the height continues. The plan records a caveat on NBGV path filters after the move.

### Consequences

* Good, because each package releases on its own schedule, and its version changes only when its own files change.
* Good, because a tag names its package, and one pattern covers every future package.
* Good, because "Latest" is set explicitly and stays on the stable .NET release; by default `gh release create` sets it from date and version.
* Bad, because generated release notes list every pull request merged between the two tags, including those of the other packages: `--notes-start-tag` sets only the starting point. The release notes configuration in `.github/release.yml` can exclude pull requests by label or author, not by path.
* Bad, because two tag formats coexist in the history; the legacy MCP tags `DaxFormatter_MCP_v*` match neither `*-v*` nor `v*`, so the ruleset covers them only with a pattern of their own.
* Bad, because the MCP server's version jumps from 0.1 to 0.2-beta for a change that is not functional.

### Confirmation

- Each release workflow reads the version with NBGV from its package's toolchain root, builds the tag from the published id and that version, and passes the release flags above; `actionlint` and a dry-run release check it.
- Review: a new package adds its own `version.json` at its toolchain root and a release workflow that follows the tag pattern.

## Pros and Cons of the Options

### One shared version and tag for all packages

* Good, because one tag marks one snapshot of every package, and release notes cover everything.
* Bad, because every package is released again when any one changes, and a beta package and a stable one must share a prerelease state.
* Bad, because the published versions already diverge (1.2.2 and 0.1.4-beta), so a shared version would renumber at least one published package.

### Independent versions, keeping today's ad-hoc tag names

* Good, because existing tags and workflows stay as they are.
* Bad, because each new package invents its own format, and no single pattern covers them: `v*` doesn't say which package, and `DaxFormatter_MCP_v*` follows no rule a new package could reuse.
* Bad, because `v<version>` reads as the version of the whole repository once there are several packages.

### Independent versions with one `version.json` per package and `<published id>-v<version>` tags

* Good, because the tag carries the id users see on nuget.org and npm.
* Good, because one `version.json` per folder with path filters is the setup NBGV documents and both packages already use.
* Neutral, because the npm scope is left out of the tag: `*` in ruleset patterns doesn't match its `/`.
* Bad, because each release must find its package's previous tag for `--notes-start-tag`, and the notes still include other packages' pull requests.

## More Information

- The layout this decision versions: [ADR-0001](0001-organize-the-repository-by-language.md).
- NBGV: [`version.json`](https://dotnet.github.io/Nerdbank.GitVersioning/docs/versionJson.html) and [path filters](https://dotnet.github.io/Nerdbank.GitVersioning/docs/path-filters.html), relative to the `version.json` that declares them.
- [`gh release create`](https://cli.github.com/manual/gh_release_create): `--latest`, `--prerelease`, `--notes-start-tag`, `--generate-notes`, `--draft`.
- [Creating rulesets for a repository](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/creating-rulesets-for-a-repository): tag rulesets and `fnmatch` patterns.
- [Automatically generated release notes](https://docs.github.com/en/repositories/releasing-projects-on-github/automatically-generated-release-notes): merged pull requests, contributors, and exclusions by label or author.
- Revisit if packages need coordinated releases, or if a release tool replaces the release workflows.
