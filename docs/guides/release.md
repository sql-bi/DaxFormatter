# How to release a package

Publish a new version of `Dax.Formatter` to nuget.org or of `@sqlbi/daxformatter-mcp` to npm. Each package releases on its own, from its own workflow ([ADR-0002](../decisions/0002-version-and-tag-each-package-independently.md)); how versions are computed is in the [architecture](../design/architecture.md#versions-and-releases).

## Before you start

- You can run workflows in this repository.
- The change to release is merged into `master`, the only branch a release runs from.
- The version is new. NBGV raises it with every commit under the package's path filter, so there is nothing to bump for a patch; to change the major, minor or prerelease part, change `version` in the package's `version.json` in a pull request. To see the version a release would publish, on an up-to-date `master`:
  - .NET, from `dotnet/`:
    ```bash
    dotnet msbuild ./src/Dax.Formatter/Dax.Formatter.csproj -restore -target:GetBuildVersion -getProperty:NuGetPackageVersion
    ```
  - MCP server, from `typescript/packages/daxformatter-mcp/`, after `npm ci`, with .NET on the `PATH`, which the nbgv CLI runs on:
    ```bash
    npx --no nbgv get-version --variable NpmPackageVersion
    ```
- Set up once by a maintainer:
  - .NET: on nuget.org, a Trusted Publishing policy for this repository and the workflow file `dotnet-release-dax-formatter.yml`; the repository secrets `NUGET_USER`, the nuget.org user name (profile name, not the email address) that `NuGet/login` signs in with, and `SIGNING_KEY`, the strong-name key in base64.
  - MCP server: the repository secret `NPM_TOKEN`, an npm token that can publish `@sqlbi/daxformatter-mcp`.

| Package | Workflow file | Tag | Release title | Release flags |
|---|---|---|---|---|
| `Dax.Formatter` | `dotnet-release-dax-formatter.yml` | `Dax.Formatter-v<version>` | `DaxFormatter v<version>` | `--latest` |
| `@sqlbi/daxformatter-mcp` | `typescript-release-daxformatter-mcp.yml` | `daxformatter-mcp-v<version>` | the tag | `--prerelease --latest=false` |

Actions shows each workflow under its file name without `.yml`.

## Steps

1. Run the package's release workflow from `master` with `dry_run` checked: in GitHub, Actions, the workflow, "Run workflow", branch `master`. From a terminal with the GitHub CLI:
   ```bash
   gh workflow run dotnet-release-dax-formatter.yml --ref master -f dry_run=true
   gh workflow run typescript-release-daxformatter-mcp.yml --ref master -f dry_run=true
   ```
   A dry run builds, tests and packs, and publishes nothing. It prints the tag, the previous tag, and the `gh release create` command that the release would run, and fails if the version is already tagged. The .NET dry run still signs in to nuget.org, so it checks the Trusted Publishing policy; the MCP dry run lists the package's files with `npm pack --dry-run`.
2. When the dry run passes, run the same workflow with `dry_run` unchecked (`-f dry_run=false`). The workflow:
   - .NET: reads the version and the tags, writes the signing key from `SIGNING_KEY` and fails if it is missing, builds, runs the tests against the live service, packs, signs in to nuget.org, pushes `Dax.Formatter.<version>.nupkg`, and creates the GitHub release.
   - MCP server: installs with `npm ci`, reads the version and the tags, builds, tests, writes the version into `package.json` with `nbgv-setversion`, runs `npm publish --provenance --access public`, and creates the GitHub release.
   - The GitHub release is a draft with the tag, title and flags of the table, on the commit the workflow ran on. Its notes start from the previous tag: the package's nearest tag on the history of `master`, in the form of the table or in the package's legacy form ([ADR-0002](../decisions/0002-version-and-tag-each-package-independently.md)).
3. In GitHub, Releases, open the draft. GitHub generates its notes from the merged pull requests, which can include those of the other package: remove the lines that don't belong to this package.
4. Publish the draft before the package's next release, whose notes start from this one. The workflow already set the flags, but GitHub doesn't document that a draft keeps them, so set them again:
   - .NET: check "Set as the latest release".
   - MCP server: check "Set as a pre-release" and leave "Set as the latest release" unchecked, so that "Latest" stays on the .NET client.

## Check

- The version is on [nuget.org](https://www.nuget.org/packages/Dax.Formatter) or [npm](https://www.npmjs.com/package/@sqlbi/daxformatter-mcp); `npm view @sqlbi/daxformatter-mcp version` prints it. nuget.org may take a few minutes to index a new version.
- `gh release list` shows the release, published with the tag of the table; the latest .NET release is "Latest", and the MCP releases are "Pre-release". If "Latest" is on another release, run `gh release edit Dax.Formatter-v<version> --latest` with the latest .NET version; if an MCP release is not a pre-release, run `gh release edit daxformatter-mcp-v<version> --prerelease`.

## If something goes wrong

- "Releases run only from refs/heads/master": run the workflow again with branch `master`.
- "No previous ... tag reachable from this commit": the release notes have no starting point. The package's tags, new or legacy, must be on the history of the commit the workflow ran on, and the checkout must keep `fetch-depth: 0`, which also fetches them.
- "The SIGNING_KEY secret is missing or empty": a maintainer sets the secret, then run again.
- The nuget.org sign-in fails: the Trusted Publishing policy must name `dotnet-release-dax-formatter.yml` and this repository.
- "Tag ... already exists", or `dotnet nuget push` or `npm publish` fails because the version exists: no commit has touched the package's path filter since the last release. Merge the change to release first.
- The package is published but the GitHub release is missing: run the command that the run printed in its "create github release" step, or create the draft by hand on the commit the workflow ran on, with the tag, title and flags of the table and the previous tag that its "read tags" step printed; then continue from step 3. If GitHub rejects `--latest` on a draft, run the command without it: step 4 sets it at publish.
  ```bash
  gh release create "<tag>" --title "<title>" --target <commit> --generate-notes --notes-start-tag <previous tag> <flags> --draft
  ```
- A published version can't be replaced: on nuget.org it can only be unlisted. Fix the problem and release the next version.
