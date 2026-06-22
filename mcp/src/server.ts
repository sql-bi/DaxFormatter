import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import {
    type DaxFormatServiceFactory,
    FORMAT_DAX_TOOL_NAME,
    defaultServiceFactory,
    formatDaxInputSchema,
    formatDaxOutputSchema,
    runFormatDax,
} from "./format-tool";
import { SERVER_NAME, SERVER_VERSION } from "./version";

const INSTRUCTIONS = `# DAX Formatter MCP

Formats DAX (Data Analysis Expressions) for Power BI, Analysis Services, and Tabular models via the SQLBI daxformatter.com service. This is the single canonical channel for DAX formatting.

Rules:
- Canonical channel: when DAX needs formatting, use this server's tools; never call daxformatter.com directly.
- Input integrity: send the user's DAX to the tool exactly as provided, byte-for-byte; do not alter, "fix", or reformat it first. Propose or apply changes only if the user explicitly asked.
- Surface errors verbatim: when the tool returns syntax errors, relay them to the user exactly as received (line, column, message); they are diagnostics the user needs.`;

const TOOL_DESCRIPTION = `Format and validate one or more DAX expressions with the DAX Formatter service (daxformatter.com). \
Use it whenever DAX needs pretty-printing or a syntax check; several expressions can be formatted in a single call. \
Returns one result per input expression, in order: the formatted text plus any syntax errors (line, column, message). \
An expression that cannot be parsed comes back with a null 'formatted' value and a non-empty 'errors' list. \
Syntax errors are normal results here, not a tool failure.`;

/**
 * Builds a configured MCP server. The DAX Formatter service factory is injectable so tests can
 * supply a fake; production uses {@link defaultServiceFactory}.
 */
export function createServer(factory: DaxFormatServiceFactory = defaultServiceFactory): McpServer {
    const server = new McpServer(
        { name: SERVER_NAME, version: SERVER_VERSION },
        { instructions: INSTRUCTIONS },
    );

    server.registerTool(
        FORMAT_DAX_TOOL_NAME,
        {
            title: "Format DAX",
            description: TOOL_DESCRIPTION,
            inputSchema: formatDaxInputSchema,
            outputSchema: formatDaxOutputSchema,
            annotations: { readOnlyHint: true, idempotentHint: true, openWorldHint: true },
        },
        (args) =>
            runFormatDax(args, {
                factory,
                serverVersion: SERVER_VERSION,
                resolveClientInfo: () => server.server.getClientVersion(),
            }),
    );

    return server;
}
