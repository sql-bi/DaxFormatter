import { Client } from "@modelcontextprotocol/sdk/client/index.js";
import { InMemoryTransport } from "@modelcontextprotocol/sdk/inMemory.js";
import { describe, expect, it, vi } from "vitest";
import type { DaxFormatServiceFactory } from "../src/format-tool";
import { createServer } from "../src/server";
import { SERVER_VERSION } from "../src/version";

describe("createServer (integration)", () => {
    it("exposes format_dax and routes a call to the service with the caller identity", async () => {
        const format = vi.fn(async (expressions: string[]) =>
            expressions.map((expression) => ({ formatted: `${expression}!`, errors: [] })),
        );
        const factory = vi.fn(() => ({ format })) as unknown as DaxFormatServiceFactory;

        const server = createServer(factory);
        const [clientTransport, serverTransport] = InMemoryTransport.createLinkedPair();
        await server.connect(serverTransport);

        const client = new Client({ name: "test-client", version: "1.2.3" });
        await client.connect(clientTransport);

        const { tools } = await client.listTools();
        expect(tools.map((t) => t.name)).toContain("format_dax");

        const result = await client.callTool({
            name: "format_dax",
            arguments: { expressions: ["a", "b"] },
        });

        expect(result.structuredContent).toEqual({
            results: [
                { formatted: "a!", errors: [] },
                { formatted: "b!", errors: [] },
            ],
        });
        expect(factory).toHaveBeenCalledWith(
            "daxformatter-mcp-local/test-client",
            `${SERVER_VERSION}/1.2.3`,
        );

        await client.close();
        await server.close();
    });
});
