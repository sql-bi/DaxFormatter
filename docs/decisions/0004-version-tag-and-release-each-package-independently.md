---
status: accepted
date: 2026-11-19
decision-makers: Alberto Spelta
---

# Version, tag and release each package independently

## Context and Problem Statement

[ADR-0002](0002-version-and-tag-each-package-independently.md) gave each package its own NBGV version and `<published id>-v<version>` tags, and had each release pass `--notes-start-tag` with the package's previous tag, confirmed by a dry-run release. Putting it into the release workflows ([`language-first-layout.md`](https://github.com/sql-bi/DaxFormatter/blob/4928eacfedfcd2be7a0f7807162b505f8242af11/docs/plans/language-first-layout.md), PR 4) showed three things. Generated release notes list every pull request merged since their starting point, of any package, so a maintainer edits them in the draft release anyway. Finding the previous tag takes `git describe` with a tag pattern per package, the full history and a special case for a package's first release. And GitHub doesn't document whether a draft keeps the Latest flag passed when it is created. The maintainers also removed the release dry run. Keeping the versions and tags of ADR-0002, how should a release set its notes and its flags?

## Considered Options

* Pass `--notes-start-tag` with the package's previous tag, found by the release workflow
* Let GitHub generate the notes from its own starting point, and edit them in the draft

## Decision Outcome

Chosen option: "Let GitHub generate the notes from its own starting point, and edit them in the draft", because the draft is edited before publishing either way, and the release workflows then need no tag lookup, no tag pattern per package and no special case for a first release.

- Each package has one NBGV `version.json` at its toolchain root, with path filters, so that only changes to the package raise its version height.
- Tags are `<published id>-v<version>`, where the id is the NuGet package id or the npm package name without its `@sqlbi/` scope: `Dax.Formatter-v1.2.3`, `daxformatter-mcp-v0.2.1-beta`.
- Existing tags stay as they are: `v<version>` for the .NET client and `DaxFormatter_MCP_v<version>` for the MCP server.
- A tag ruleset protects `*-v*`, `v*` and the legacy `DaxFormatter_MCP_v*`.
- Each release workflow creates a draft GitHub release on the released commit with `gh release create --generate-notes --draft` and no `--notes-start-tag`: the .NET client with `--latest`, the MCP server with `--prerelease --latest=false`.
- Before publishing a draft, a maintainer edits its notes down to the package's changes since its previous release, and sets the same flags again.
- A release has no dry run: every check runs before the package is pushed to its registry, so a run that fails a check publishes nothing.

### Consequences

* Good, because each package releases on its own schedule, its version changes only when its own files change, and a tag names its package, as in ADR-0002.
* Good, because a release workflow builds its tag from the published id and the version alone, so a new package's workflow needs no tag pattern and no first-release case.
* Good, because "Latest" is set explicitly and stays on the stable .NET release; by default `gh release create` sets it from date and version.
* Bad, because GitHub doesn't document where generated notes start, so the notes of every draft must be checked: a draft published unedited can list another package's pull requests or the wrong range.
* Bad, because the flags are set twice, when the draft is created and when it is published, as long as GitHub doesn't document whether a draft keeps them.
* Bad, because two tag formats coexist in the history, and the legacy MCP tags need a ruleset pattern of their own.
* Bad, because a run for a version already released is stopped only by `dotnet nuget push` or `npm publish`, after the build and the tests, though still before the GitHub release.

### Confirmation

- Review: each release workflow reads the version with NBGV from its package's toolchain root, builds the tag from the published id and that version, and passes the flags above with `--draft`; `actionlint` checks the workflows.
- After a release is published, `gh release list` shows the latest .NET release as "Latest" and the MCP releases as "Pre-release".
- Review: a new package adds its own `version.json` at its toolchain root and a release workflow that follows the tag pattern.

## Pros and Cons of the Options

### Pass `--notes-start-tag` with the package's previous tag, found by the release workflow

* Good, because the generated notes start at the package's own previous release.
* Bad, because the notes still list the other packages' pull requests merged since then, so the draft is edited anyway.
* Bad, because the workflow needs the tags fetched with the full history, a `git describe` pattern per package that no other package's tags may match, and a special case for a package's first release, which has no previous tag.

### Let GitHub generate the notes from its own starting point, and edit them in the draft

* Good, because the workflow passes only the tag, the title and the flags.
* Bad, because the starting point is GitHub's choice, which its documentation doesn't describe.

## More Information

- Supersedes [ADR-0002](0002-version-and-tag-each-package-independently.md), which also passed `--notes-start-tag` with the package's previous tag and named a dry-run release under Confirmation. Its Context and its options for versions and tags (one shared version, ad-hoc tag names) still explain why each package has its own version and tags, and it records why the MCP server's version moved to 0.2-beta with the layout of ADR-0001.
- The layout these packages live in: [ADR-0001](0001-organize-the-repository-by-language.md).
- NBGV: [`version.json`](https://dotnet.github.io/Nerdbank.GitVersioning/docs/versionJson.html) and [path filters](https://dotnet.github.io/Nerdbank.GitVersioning/docs/path-filters.html).
- [`gh release create`](https://cli.github.com/manual/gh_release_create): `--generate-notes`, `--latest`, `--prerelease`, `--draft`.
- [Automatically generated release notes](https://docs.github.com/en/repositories/releasing-projects-on-github/automatically-generated-release-notes) and [Create a release](https://docs.github.com/en/rest/releases/releases#create-a-release), whose `make_latest` reads "Drafts and prereleases cannot be set as latest".
- [Creating rulesets for a repository](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/creating-rulesets-for-a-repository): tag rulesets and `fnmatch` patterns.
- Revisit if GitHub documents where generated notes start or lets them exclude pull requests by path, or if a release tool replaces the release workflows.
