---
status: accepted
date: 2026-11-17
decision-makers: Alberto Spelta, Marco Russo
---

# Organize the repository by language

## Context and Problem Statement

The repository publishes two packages built with different toolchains: the .NET client `Dax.Formatter` on NuGet, whose solution and build settings sit in `src/` and whose SDK pin, package sources, version file, icon and readme sit at the repository root; and the MCP server `@sqlbi/daxformatter-mcp` on npm, in `mcp/`, which carries its own TypeScript port of the client. A TypeScript client library and a Python client are planned, and the repository is meant to be the public reference for DAX Formatter users. The root is today both the .NET toolchain root and the repository's front page, so a new language or package has no clear place. How should the repository be laid out so that each package builds, versions and releases on its own? The restructuring is planned in [`language-first-layout.md`](../plans/language-first-layout.md).

## Considered Options

* Keep today's layout: .NET in `src/` and at the root, the MCP server in `mcp/`
* Group by package kind: top-level folders by role (client libraries, MCP servers), with the language below
* Group by language: one top-level folder per language, each a self-contained toolchain root

## Decision Outcome

Chosen option: "Group by language", because each language folder is one toolchain root, so the files that tools look up in parent folders apply to one language only, and CI, Dependabot and code owners map one-to-one to folders. Multi-language repositories with the same need use this layout (see More Information).

- The root holds only governance (license, community files, agent instructions), documentation and CI.
- `dotnet/` is the .NET toolchain root: solution, build settings, SDK pin, package sources, version file, and the NuGet readme and icon, with projects under `src/`, `tests/` and `samples/`.
- Each npm package is a toolchain root of its own under `typescript/packages/<package>`, with its manifest, lockfile and version file; the MCP server is `typescript/packages/daxformatter-mcp`.
- Later, `python/` holds the Python client and `spec/` the OpenAPI contract shared by all clients (see the [backlog](../backlog.md)).
- Workflows carry as a prefix the language folder they work in. A CI workflow builds and tests the whole folder and filters on its paths: `dotnet-ci.yml`, `typescript-ci.yml`. A release workflow also names its package, since each package releases on its own ([ADR-0002](0002-version-and-tag-each-package-independently.md)): `<language>-release-<package>.yml`, where the package is the id of its tags in lowercase, with dashes for dots: `dotnet-release-dax-formatter.yml`, `typescript-release-daxformatter-mcp.yml`.
- The default branch stays `master`, because renaming it is not needed for the layout and `master` is named in both packages' NBGV `publicReleaseRefSpec` and in the workflow triggers. Package ids don't change.

### Consequences

* Good, because each language builds, tests and releases from its own folder, and a new package or language gets a place without touching the others.
* Good, because the root `README.md` can become a landing page for all DAX Formatter users instead of the NuGet package readme.
* Bad, because links to `master` paths on GitHub break, including the `homepage` of the npm package already published, until its next release; the NuGet project URL points at the repository root and stays valid.
* Bad, because the readme of every published NuGet version shows the CI badge of `ci.yml`, which breaks when the workflow is renamed; only the next release carries the new badge.
* Bad, because open branches and pull requests that touch moved files must be rebased onto the move.
* Bad, because the history of a moved file before the move needs `git log --follow`.
* Bad, because the nuget.org Trusted Publishing policy names the workflow file, so renaming `release.yml` to `dotnet-release-dax-formatter.yml` needs a new policy before the next .NET release, and each new NuGet package, with its own release workflow, needs one too.
* Bad, because the MCP package's NBGV version height restarts at the move, so its version moves to 0.2-beta ([ADR-0002](0002-version-and-tag-each-package-independently.md)).

### Confirmation

- Review: no toolchain, package or version files at the root; a new package lives in its language folder with its own toolchain files.
- CI: each CI workflow runs in its language folder and triggers only on changes to that folder and to its own workflow file.

## Pros and Cons of the Options

### Keep today's layout: .NET in `src/` and at the root, the MCP server in `mcp/`

* Good, because nothing moves: links, open branches, the Trusted Publishing policy and version heights stay as they are.
* Good, because the two CI workflows are already kept apart by path filters (`src/**`, `mcp/**`).
* Bad, because the root is the .NET toolchain root, and its `README.md` is the NuGet package readme, so the repository's front page describes only the .NET client.
* Bad, because a TypeScript client, npm workspaces or a Python client have no obvious place, and `src/` reads as the source of the whole repository while it holds only .NET.

### Group by package kind: top-level folders by role (client libraries, MCP servers), with the language below

* Good, because the top level says what each package does, whatever its language.
* Bad, because a toolchain is split across trees: the TypeScript client and the TypeScript MCP server would sit in different folders, so npm workspaces, one lockfile or a shared TypeScript configuration would need a root above both, at the repository root.
* Bad, because CI, Dependabot and code owners are configured per toolchain, so their entries would list folders across trees.
* Neutral, because none of the repositories checked under More Information uses it.

### Group by language: one top-level folder per language, each a self-contained toolchain root

* Good, because SDK pin, package sources, version file and lockfile resolve from the nearest language folder, and no toolchain file sits at the root.
* Good, because npm workspaces can be rooted at `typescript/` when a second npm package arrives.
* Good, because it is the layout of the multi-language repositories checked under More Information.
* Bad, because a reader must know a package's language to find it; a package table in the root `README.md` answers that.
* Bad, because the move has a one-time cost, listed under Consequences.

## More Information

- Versions and tags per package: [ADR-0002](0002-version-and-tag-each-package-independently.md).
- Precedents, top-level folders checked on 2026-11-16 with the GitHub contents API: [microsoft/agent-framework](https://github.com/microsoft/agent-framework) (`dotnet/`, `python/`, `go/`), [microsoft/semantic-kernel](https://github.com/microsoft/semantic-kernel) (`dotnet/`, `python/`, `java/`), [microsoft/autogen](https://github.com/microsoft/autogen) (`dotnet/`, `python/`), [microsoft/TypeChat](https://github.com/microsoft/TypeChat) (`typescript/`, `python/`, `dotnet/`).
- [Trusted Publishing on nuget.org](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing): a GitHub Actions policy names the workflow file.
- [npm `package.json`, `repository`](https://docs.npmjs.com/cli/v11/configuring-npm/package-json#repository): a package in a monorepo declares its folder in `repository.directory`.
- Revisit if a package needs another language's code at build time, or if a package moves to a repository of its own.
