import { readFileSync } from "node:fs";

// The version is owned by Nerdbank.GitVersioning (see version.json) and stamped into package.json
// at pack time; the committed package.json carries a `0.0.0-placeholder`. Read it back relative to
// this module (rather than duplicating the literal, which would silently drift) — the URL resolves
// both from the bundled dist (`dist/<entry>.js` → `../package.json`) and from source during tests.
const { version } = JSON.parse(
    readFileSync(new URL("../package.json", import.meta.url), "utf8"),
) as { version: string };

// The MCP server's advertised name (the `serverInfo.name` returned at `initialize`). Kept in
// sync with the .NET MCP's `host.json` `serverName`.
export const SERVER_NAME = "DAX Formatter MCP";

// This MCP's own version (from package.json), reported as the first segment of callerVersion.
export const SERVER_VERSION = version;

// Base of the `callerApp` wire identity (anonymous usage stats). Identifies this local (stdio) MCP
// so it's distinguishable from the remote HTTP one in the stats; runFormatDax appends the connected
// client's name → "daxformatter-mcp-local/<client>". (The .NET HTTP MCP uses "daxformatter-mcp-http".)
export const CALLER_APP = "daxformatter-mcp-local";
