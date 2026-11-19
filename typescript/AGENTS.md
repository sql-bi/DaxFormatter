# AGENTS.md

npm packages, one folder each under `packages/`. Today there is one: `packages/daxformatter-mcp`, the stdio MCP server `@sqlbi/daxformatter-mcp`, which carries its own TypeScript port of the DAX Formatter client in `src/daxformatter/`. This file adds the rules of `typescript/` to the root [`AGENTS.md`](../AGENTS.md).

## Commands

Run from the package folder: there are no npm workspaces, so each package installs and builds on its own.

```bash
cd packages/daxformatter-mcp
npm ci
npm run typecheck
npm run build
npm test
npm pack --dry-run
# the version a release would publish, when run on an up-to-date master; elsewhere it has a commit suffix
# (the nbgv CLI runs on .NET, which must be on the PATH)
npx --no nbgv get-version --variable NpmPackageVersion
```

## Done means

- `npm run typecheck`, `npm run build` and `npm test` pass on a current release of the Node.js major that `.github/workflows/typescript-ci.yml` sets: the build and test tools need a newer Node.js than the published package, which keeps working on the versions in `engines`.
- `npm pack --dry-run` still lists only `dist/**`, `README.md` and `package.json`.

## Boundaries

- Ask first: changes to `name`, `bin`, `files` or `engines` in `package.json`; the tool's name, description, input or output schema, which AI clients depend on.
- Never: set `version` in `package.json`. It holds a placeholder; the release workflow writes the version computed by NBGV.
- Never: write to stdout from the server outside the MCP protocol: stdout carries the stdio transport, so log to stderr.
- Never: call the network from tests: inject a mock `fetch` or `DaxFormatServiceFactory`, as the existing tests do.

## When you change X, update Y

- The package README → the root rules on package READMEs and on AI client setup apply; like any change in the package folder, it raises the package's version ([architecture](../docs/design/architecture.md#versions-and-releases)).
- A second npm package → its own folder under `packages/`, then the root rule for a new package; npm workspaces rooted at `typescript/` are a backlog item.
