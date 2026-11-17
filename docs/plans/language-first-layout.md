# Language-first layout

## Goal

Reorganize the repository by language ([ADR-0001](../decisions/0001-organize-the-repository-by-language.md)) and version, tag and release each package on its own ([ADR-0002](../decisions/0002-version-and-tag-each-package-independently.md)), so that the repository becomes the public reference for DAX Formatter users and every package builds, tests, packs and releases as it does today.

Done when all of these hold:

- History: the rename commit of PR 2 shows only `rename (100%)` in `git diff -M --summary`, and `git log --follow dotnet/src/Dax.Formatter/DaxFormatterClient.cs` reaches the January 2021 history.
- .NET client: from `dotnet/`, `dotnet build`, `dotnet test` and `dotnet pack` succeed; the `.nupkg` holds the readme, icon, license and `lib/{netstandard2.0,net8.0,net10.0}`, with the same file list as the published 1.2.2; the sample builds with the same warnings as before the move; `nbgv get-version -p dotnet` continues the 1.2 line above 1.2.2 (no exact patch number: every commit under the path filter adds to the height).
- MCP server: in `typescript/packages/daxformatter-mcp`, `npm ci`, `npm run typecheck`, `npm run build` and `npm test` succeed; `npm pack --dry-run` lists only `dist/**`, `README.md` and `package.json`; `nbgv get-version -p typescript/packages/daxformatter-mcp` gives 0.2.x-beta.
- CI and releases: `actionlint` reports nothing; pull request CI is green; both release workflows succeed with `dry_run=true`, `NuGet/login` validates the new Trusted Publishing policy; Dependabot reports no errors on the new paths; GitHub shows `CODEOWNERS` as valid; `gh release list` still shows the latest .NET release as "Latest".
- Documentation: every relative link in the Markdown files resolves (`lychee --offline` on `*.md`); each `AGENTS.md` is under 200 lines and each `CLAUDE.md` contains only `@AGENTS.md`; each record has valid front matter, numbering is continuous from `0001`, and `docs/decisions/README.md` has no index; no fact has two homes (the REST contract only in `docs/design/rest-api.md`, the MCP client setup only in the home PR 3 picks, linked from the other places); at the end `docs/plans/` is empty and the backlog has no stale `Now`.

## Non-goals

- No application change: code, public API, wire format and behaviour stay as they are, including the enum divergence recorded in the [backlog](../backlog.md).
- No new packages and no OpenAPI contract yet: they are backlog items.
- No history rewrite: files move with `git mv` only.
- No change to the package ids `Dax.Formatter` and `@sqlbi/daxformatter-mcp`, to existing tags, or to the default branch `master`.

## Approach

- Plan before code: this plan and the two records, in `status: proposed`, are reviewed before any file moves; PR 2 accepts the records.
- Guard the releases first (PR 1), so that a wrong path after the move fails a release instead of publishing a misversioned package.
- Move in one rename-only commit, then fix paths in separate commits, and merge PR 2 with "Rebase and merge": the rename-only commit reaches `master` as it is, so git detects every rename at 100% and `git log --follow` reaches each file's earlier history. A squash merge would fold renames and edits into one commit.
- Write the documentation and community files after the move (PR 3), so they describe the final paths; change the release conventions last (PR 4), because they change how releases are tagged.
- Set aside: rewriting history so that files appear at their new paths in old commits. It changes every commit id of a public repository, which breaks forks, clones and the commit ids recorded in published packages. The layout and versioning alternatives are in the two records.

## Steps

- [x] PR 0, plan (`docs/language-first-layout-plan`): `docs/README.md`, `docs/decisions/README.md`, ADR-0001 and ADR-0002 in `status: proposed`, `docs/backlog.md` and this plan. Verify: relative links resolve (`lychee --offline` on `*.md`); the records have valid front matter; review approved.
- [ ] PR 1, release guards (`ci/release-guards`). Verify: `actionlint`; after the merge, a run of each release workflow with `dry_run=true` from `master` passes every guard and publishes nothing, and a run from a branch created from `master` after the merge stops at the branch guard (Discoveries).
  - Before PR 2: close or merge the open Dependabot pull requests, and park the other open branches that touch files being moved.
  - In `release.yml` and `publish-mcp.yml`: a `dry_run` input that skips publishing and the GitHub release; run only from `refs/heads/master`; read the version from the NBGV copy that stamps the package, the `Nerdbank.GitVersioning` MSBuild task and the `nbgv` CLI of the `nerdbank-gitversioning` npm package, instead of the nbgv action, so the tag and the pushed file can't differ from the packed version (Discoveries); follow GitHub's [secure use reference](https://docs.github.com/en/actions/reference/security/secure-use): actions pinned to full commit SHAs, `contents: read` by default and the write permissions on the job, no persisted git credentials, expressions passed to scripts through `env`, one run at a time with later runs queued.
  - In `release.yml`: fail if the signing key secret is missing or empty, or the `.snk` can't be written from it.
  - Maintainer: on nuget.org, add a Trusted Publishing policy for `dotnet-release.yml`, since a policy names the workflow file; remove the `release.yml` policy after the first release from `dotnet-release.yml`.
  - Maintainer: enable "Allow rebase merging", for PR 2 only.
- [ ] PR 2, move (`refactor/language-first-layout`), merged with "Rebase and merge", four commits. Verify: the Goal checks for history, the .NET client and the MCP server; in the .NET check, the version of the packed `.nupkg` equals `nbgv get-version -p dotnet` (with the nbgv CLI at the version of the `Nerdbank.GitVersioning` package), and a scratch commit under `dotnet/src/Dax.Formatter` raises it; a new file created in the MCP package shows in `git status`; `actionlint`; pull request CI green.
  - [ ] `refactor: move to language-first layout`, only `git mv`: `src/Dax.Formatter` to `dotnet/src/Dax.Formatter`, `src/Dax.Formatter.Tests` to `dotnet/tests/Dax.Formatter.Tests`, `samples/getting-started` to `dotnet/samples/GettingStarted`; `src/Dax.Formatter.sln`, `src/Directory.Build.props`, `version.json`, `global.json`, `nuget.config`, `package-icon.png` and `README.md` to `dotnet/`; `mcp` to `typescript/packages/daxformatter-mcp`; the workflows `ci.yml` to `dotnet-ci.yml`, `release.yml` to `dotnet-release.yml`, `ci-mcp.yml` to `typescript-ci.yml`, `publish-mcp.yml` to `typescript-release-mcp.yml`. Verify: `git diff -M --summary` of the commit shows only `rename (100%)`.
  - [ ] `build: update paths after move`: every row of the table under Path-dependent settings, plus `dotnet/samples/Directory.Build.props` and `dotnet/samples/Directory.Build.targets` containing only `<Project />` (a 0-byte file fails with MSB4024): today the sample inherits no build settings, and without them it would inherit `dotnet/Directory.Build.props`. Verify: from `dotnet/`, `dotnet build`, `dotnet test`, `dotnet pack`; the sample builds with the same warnings as before.
  - [ ] `build(mcp): bump version to 0.2-beta` in the MCP package's `version.json`, because the move restarts its NBGV height (ADR-0002). Verify: `nbgv get-version -p typescript/packages/daxformatter-mcp` gives 0.2.x-beta.
  - [ ] `docs: accept layout decisions`: ADR-0001 and ADR-0002 to `status: accepted` with the new `date`; Progress of this plan; a short root `README.md` that links `dotnet/README.md` and the MCP package README until PR 3, because without a root README GitHub shows `docs/README.md` as the repository's front page. Verify: the records have valid front matter; the repository page shows the short README.
  - Maintainer: merge with "Rebase and merge", then restore squash-only merging.
- [ ] PR 3, documentation and community files (`docs/repository-landing`). Verify: `lychee --offline` on `*.md`; the `AGENTS.md` and `CLAUDE.md` checks of the Goal; no fact in two homes; GitHub shows `CODEOWNERS` as valid.
  - Root `README.md` as a landing page, replacing PR 2's short one: what DAX Formatter is, a package table linking `dotnet/README.md` and the MCP package README, documentation, support, license.
  - `dotnet/README.md` stays the NuGet package readme; fix its sentence on endpoint resolution (Discoveries).
  - `docs/design/architecture.md`: the client packages, the service they call, how each package is versioned and released.
  - `docs/design/rest-api.md`, the only home of the public REST contract: endpoints, request and response fields, enums, errors, hashing of server and database names, caller identity; written from the .NET and TypeScript models.
  - `docs/guides/connect-mcp-clients.md`: the remote MCP server hosted by the DAX Formatter service and the stdio server from npm, for VS Code, Claude and Codex. The same setup is already published on docs.sqlbi.com and in the MCP package README, which npm shows (Discoveries): first pick one home; the others link it with an absolute URL, and this guide is not created if docs.sqlbi.com stays the home.
  - `docs/guides/release.md`: how to release each package.
  - `CONTRIBUTING.md` (where code goes, build and test per language, tags, how to read history across the move), `SECURITY.md`, `CODE_OF_CONDUCT.md`. Maintainer: enable private vulnerability reporting.
  - Issue forms with a component dropdown: .NET client, MCP server, formatting result (the service); a pull request template; `CODEOWNERS` by path, replacing today's single catch-all entry.
  - `AGENTS.md`, with a `CLAUDE.md` that contains only `@AGENTS.md`, at the root, in `dotnet/` and in `typescript/`, each `AGENTS.md` under 200 lines.
  - `.editorconfig` with only the indentation style (spaces) and the charset; an explicit `.gitattributes` replacing today's Visual Studio template: `* text=auto` as today, `*.png` and `*.snk` binary, `package-lock.json` `linguist-generated`.
  - Move the Visual Studio `.gitignore` to `dotnet/.gitignore` and keep at the root only editor and OS entries (such as `.vs/` and `.idea/`), so its .NET rules (`**/[Pp]ackages/*`, `[Dd]ebug/`, `[Rr]elease/`, `[Ll]og/`) stop matching under `typescript/`; the negation added in PR 2 goes with it (Discoveries).
  - `docs/README.md` gains the rows for `design/` and `guides/`.
- [ ] PR 4, release conventions (`ci/release-conventions`). Verify: the Goal checks for CI and releases.
  - In both release workflows: tags `<published id>-v<version>`; `gh release create` with `--latest` for .NET and `--prerelease --latest=false` for the MCP server, and `--notes-start-tag` with the package's previous tag (for the first release, its last legacy tag).
  - In the CI workflows: `permissions: contents: read`; actions pinned to full commit SHAs and no persisted git credentials, as in the release workflows; a `push` trigger on `master`; unique job names `dotnet-build-and-test` and `typescript-build-and-test`.
  - If checks become required: an always-running "gate" job, because a workflow skipped by its path filter leaves its required checks pending and blocks the merge.
  - Decide whether the tag ruleset also covers the legacy `DaxFormatter_MCP_v*` tags, and check that the Latest and prerelease flags hold when a draft release is published (Discoveries).
  - Update `docs/guides/release.md`.
  - Maintainer, after the merge: add the tag ruleset that protects `*-v*` and `v*`.
- [ ] PR 5, closing. Verify: `docs/plans/` is empty; the backlog has no stale `Now`; `lychee --offline` on `*.md`.
  - Move what lasts: decisions to records, design to `docs/design/`, leftovers to the backlog.
  - Delete this plan and remove its link from `Now`; point the records' links to this plan at its last version on GitHub.

### Path-dependent settings

Every row is part of PR 2's second commit. Paths are the ones after the move.

| File | From | To |
|---|---|---|
| `dotnet/Dax.Formatter.sln` | project paths `Dax.Formatter\...`, `Dax.Formatter.Tests\...` | `src\Dax.Formatter\...`, `tests\Dax.Formatter.Tests\...` |
| `dotnet/src/Dax.Formatter/Dax.Formatter.csproj` | `..\..\LICENSE.md` | `..\..\..\LICENSE.md`; the README and icon paths stay as they are |
| `dotnet/tests/Dax.Formatter.Tests/Dax.Formatter.Tests.csproj` | `..\Dax.Formatter\Dax.Formatter.csproj` | `..\..\src\Dax.Formatter\Dax.Formatter.csproj` |
| `dotnet/Directory.Build.props` | `Condition` and `AssemblyOriginatorKeyFile` of the `.snk`: `$(MSBuildThisFileDirectory)Dax.Formatter\DaxFormatter.snk` | `$(MSBuildThisFileDirectory)src\Dax.Formatter\DaxFormatter.snk`. If wrong, the build comes out unsigned without errors, because the condition is only `Exists` |
| `dotnet/version.json` | path filter `src/Dax.Formatter` | `./src/Dax.Formatter`: the same folder, written so that the file is no longer identical to the old root `version.json` (Discoveries) |
| `.gitignore` | `**/[Pp]ackages/*` also ignores new files under `typescript/packages/` | add `!/typescript/packages/*/` after that rule. `*.snk` and `artifacts/` match at any depth and need no change |
| `typescript/packages/daxformatter-mcp/package.json` | `homepage` `.../tree/master/mcp`; `repository.directory` `mcp` | `https://github.com/sql-bi/DaxFormatter/tree/master/typescript/packages/daxformatter-mcp`; `typescript/packages/daxformatter-mcp` |
| `dotnet/README.md` | CI badge of `ci.yml` | `dotnet-ci.yml` |
| `.github/workflows/dotnet-ci.yml` | `paths` `src/**`; `global-json-file: global.json`; build, test and pack paths under `./src/` | `paths` `dotnet/**` and the workflow file; `global-json-file: dotnet/global.json` (relative to the workspace root); `defaults.run.working-directory: dotnet`, with the paths relative to it |
| `.github/workflows/dotnet-release.yml` | `global-json-file: global.json`; the `.snk` written to `src/Dax.Formatter/DaxFormatter.snk`; version, build, test and pack paths under `./src/` | `global-json-file: dotnet/global.json`; `defaults.run.working-directory: dotnet`, with the paths relative to it, so the `.snk` path stays `src/Dax.Formatter/DaxFormatter.snk` and the file lands in `dotnet/src/Dax.Formatter/`; the branch guard, which runs before checkout, sets `working-directory: .`, because a step can't start in a `dotnet/` that doesn't exist yet |
| `.github/workflows/typescript-ci.yml` | `paths` `mcp/**`; `cache-dependency-path` and `working-directory` under `mcp` | `paths` `typescript/**` and the workflow file; the package path |
| `.github/workflows/typescript-release-mcp.yml` | `working-directory` under `mcp` | the package path |
| `.github/dependabot.yml` | nuget `/`; npm `/mcp` | `/dotnet`; `/typescript/packages/daxformatter-mcp`; github-actions `/` stays |

## Progress

- 2026-11-16: PR 0 opened; next, PR 1.
- 2026-11-16: PR 0 merged as `67dec46` (#48); PR 1 opened.

## Discoveries

- `.gitignore`: the Visual Studio rule `**/[Pp]ackages/*` matches `typescript/packages/<package>`: after the move git silently ignores new files in the MCP package, while files already tracked are unaffected (checked with `git check-ignore` on a simulated move). The negation in the settings table fixes it; the package's own `.gitignore` still ignores `node_modules/` and `dist/`. Other Visual Studio rules, such as `[Dd]ebug/`, `[Rr]elease/` and `[Ll]og/`, would still match folders under `typescript/`, hence the split in PR 3.
- The client setup of the remote and stdio MCP servers, for VS Code, Claude and Codex, is already published on docs.sqlbi.com ([remote](https://docs.sqlbi.com/dax-formatter/mcp-http), [stdio](https://docs.sqlbi.com/dax-formatter/mcp-stdio)) and in the MCP package README, so a guide in this repository would be a third copy.
- NBGV and the moved `dotnet/version.json`: on a simulated move, while `dotnet/version.json` is identical to the old root file, NBGV keeps resolving its relative path filter at the old root location, so commits under `dotnet/src/Dax.Formatter` don't raise the height and the version stays at 1.2.2, the published one (nbgv CLI 3.7.112 and 3.10.94, `Nerdbank.GitVersioning` 3.9.50 MSBuild task). Once the file's content changes, the filter resolves under `dotnet/` and the height continues from the 1.2 history.
- NBGV versions disagree after the move: on the simulated move, the nbgv CLI 3.10.94 counts the rename commit and the 3.9.50 MSBuild task doesn't, so their versions differ by one; the same NBGV version (3.10.94 for both) agrees. The nbgv action installs the latest CLI unless `toolVersion` is set, while the package version comes from the `Nerdbank.GitVersioning` package (.NET) and the `nerdbank-gitversioning` npm package (MCP). On a mismatch the .NET push looks for a `.nupkg` name that doesn't exist, and the MCP release would tag a version other than the one published to npm. Hence PR 1 reads the version from the copy that stamps each package and drops the nbgv action; `v0.4` of the action also runs whatever `nbgv` is on the `PATH` when the requested version can't be installed.
- The copies that stamp the packages also report the version: `dotnet msbuild <project> -restore -target:GetBuildVersion -getProperty:NuGetPackageVersion` prints it, and the `nerdbank-gitversioning` npm package exposes the `nbgv` CLI next to `nbgv-setversion`, so `npx --no nbgv get-version --variable <name>` runs the CLI that stamps `package.json`. Dependabot bumps each copy with its package.
- The GitHub-hosted runner images ship .NET 8 to 11 and the nbgv CLI 3.10.94 ([Ubuntu 24.04](https://github.com/actions/runner-images/blob/main/images/ubuntu/Ubuntu2404-Readme.md), [Windows Server 2025](https://github.com/actions/runner-images/blob/main/images/windows/Windows2025-Readme.md)). The CLI in the npm package sets `rollForwardOnNoCandidateFx: 2` (major roll forward), so it runs on the runner's .NET and the MCP release needs no setup-dotnet.
- On a simulated `master` run, the .NET read gives `1.2.2` and the MCP read gives `0.1.4-beta`; on a branch, both carry a `-g<commit>` suffix. Both versions are already published, since no later commit touches the path filters, so a real release now fails at `dotnet nuget push` or `npm publish`; merging a Dependabot pull request under `mcp/` raises the MCP one.
- Dependabot updates actions pinned to a commit SHA, and the version comment on the same line ([supported ecosystems](https://docs.github.com/en/code-security/dependabot/ecosystems-supported-by-dependabot/supported-ecosystems-and-repositories#github-actions)).
- The open Dependabot majors, `actions/checkout` v7, `actions/setup-dotnet` v6 and `actions/setup-node` v7, already run on Node 24 and keep the inputs used here; the only change that affects these workflows is that setup-node v7 no longer exports a dummy `NODE_AUTH_TOKEN` when `registry-url` is set. The publish step sets its own token and `npm pack --dry-run` worked without one (npm 11, locally), but setup-node's README warns that older npm versions may fail without it and the MCP release runs `npm ci` with that `.npmrc`: after that bump, release with `dry_run` first.
- A workflow run uses the workflow file of the commit it runs on ([Workflows](https://docs.github.com/en/actions/concepts/workflows-and-actions/workflows)), and `workflow_dispatch` runs on the last commit of the selected branch. The branch guard therefore stops only runs from branches that contain it: an older branch still runs its unguarded release workflow with the repository secrets, and the nuget.org Trusted Publishing policy names the workflow file, not a branch. Parking the old branches narrows this. A release job in an environment limited to `master` would close it, with the secrets in that environment and the environment named in the nuget.org policy ([Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)), because `NuGet/login` uses the job's OIDC token, not a secret.
- The legacy MCP tags `DaxFormatter_MCP_v*` match neither `*-v*` nor `v*`; PR 4 decides whether the ruleset also covers them.
- Both release workflows create draft releases (`--draft`) without the Latest or prerelease flags; the published MCP releases are marked prerelease, set when the draft was published. PR 4 checks that the flags passed at creation still hold when a draft is published.
- `ci.yml` has no `permissions` block; `ci-mcp.yml` already has `contents: read`.
- The NuGet readme says the client resolves the server endpoint once and reuses it; the client calls fixed endpoint URLs. PR 3 fixes the sentence.
- The release workflows run only on `workflow_dispatch` and have no path filters; only the CI workflows do.

## Decision log

- 2026-11-16: PR 2 also changes `dotnet/version.json` and `.gitignore`, and PR 1 pins and checks the nbgv CLI version, after the simulated move showed the problems under Discoveries.
- 2026-11-16: after review, squash-only merging comes back right after PR 2 instead of after PR 4, so PR 3 and PR 4 can't be rebase-merged by mistake; PR 2 adds a short root `README.md` until PR 3; PR 3 splits `.gitignore` and picks one home for the MCP client setup.
- 2026-11-16: `dry_run` defaults to `false`, so the one-click release stays as it is. A dry run still runs `NuGet/login`, because it validates the Trusted Publishing policy. The MCP dry run lists the package with `npm pack --dry-run`, which npm documents for that purpose and which needs no registry, token or provenance, rather than `npm publish --dry-run`.
- 2026-11-16: the guards are failing steps, the branch guard first, not job conditions, so a run from another branch shows as failed instead of skipped.
- 2026-11-17: after review, PR 1 reads the version from the NBGV copy that stamps each package instead of pinning the nbgv action's `toolVersion` to that copy and checking that the two agree: one copy can't disagree with itself, and the lock file parsing, the pin and the check go away. The MCP release drops setup-dotnet and the npm cache: the runner images ship .NET, and a release installs only what the lock file names. The release workflows follow GitHub's secure use reference; the CI workflows follow in PR 4. No public key token check, to keep the workflow simple: a wrong `.snk` path in `Directory.Build.props` after PR 2 builds an unsigned package without errors (Path-dependent settings), and no step of the release catches it. No `PublicRelease` check: the branch guard already requires `master`, the only ref in `publicReleaseRefSpec`. No check that the version is new: without `--skip-duplicate`, `dotnet nuget push` and `npm publish` fail on a version already published, before the GitHub release. `concurrency` uses `queue: max`, because the default queue cancels a pending release when another run is dispatched.
