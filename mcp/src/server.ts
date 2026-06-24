import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import {
    type DaxFormatServiceFactory,
    FORMAT_DAX_TOOL_DESCRIPTION,
    FORMAT_DAX_TOOL_NAME,
    defaultServiceFactory,
    formatDaxInputSchema,
    formatDaxOutputSchema,
    runFormatDax,
} from "./format-tool";
import { SERVER_NAME, SERVER_VERSION } from "./version";

/**
 * Builds a configured MCP server. The DAX Formatter service factory is injectable so tests can
 * supply a fake; production uses {@link defaultServiceFactory}.
 */
export function createServer(factory: DaxFormatServiceFactory = defaultServiceFactory): McpServer {
    const server = new McpServer({ name: SERVER_NAME, version: SERVER_VERSION });

    server.registerTool(
        FORMAT_DAX_TOOL_NAME,
        {
            title: "Format DAX",
            description: FORMAT_DAX_TOOL_DESCRIPTION,
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
