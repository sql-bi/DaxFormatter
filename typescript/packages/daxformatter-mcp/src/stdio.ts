import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { createServer } from "./server";

async function main(): Promise<void> {
    const server = createServer();
    const transport = new StdioServerTransport();
    await server.connect(transport);
}

main().catch((error) => {
    // stdout is reserved for the protocol; log to stderr.
    console.error(error);
    process.exit(1);
});
