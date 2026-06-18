import { describe, expect, it, vi } from "vitest";
import { type DaxFormatServiceFactory, runFormatDax } from "../src/format-tool";

describe("runFormatDax", () => {
    it("maps options, formats, and stamps callerApp from the client name", async () => {
        const format = vi.fn(async () => [{ formatted: "EVALUATE\r\n", errors: [] }]);
        const factory = vi.fn(() => ({ format })) as unknown as DaxFormatServiceFactory;

        const result = await runFormatDax(
            { expressions: ["evaluate('Table')"], serverName: "MyServer", lineStyle: "shortLine" },
            {
                factory,
                serverVersion: "9.9.9",
                resolveClientInfo: () => ({ name: "claude-ai", version: "1.2.3" }),
            },
        );

        expect(factory).toHaveBeenCalledWith("daxformatter-mcp-local/claude-ai", "9.9.9/1.2.3");
        expect(format).toHaveBeenCalledWith(["evaluate('Table')"], {
            serverName: "MyServer",
            lineStyle: "shortLine",
        });
        expect(result.structuredContent).toEqual({
            results: [{ formatted: "EVALUATE\r\n", errors: [] }],
        });
        expect(result.isError).toBeUndefined();
    });

    it("leaves the client segment empty when the client is unknown", async () => {
        const factory = vi.fn(() => ({
            format: vi.fn(async () => []),
        })) as unknown as DaxFormatServiceFactory;

        await runFormatDax(
            { expressions: ["[X] := 1"] },
            { factory, serverVersion: "1", resolveClientInfo: () => undefined },
        );

        expect(factory).toHaveBeenCalledWith("daxformatter-mcp-local", "1");
    });

    it("returns a tool error when the service throws", async () => {
        const factory: DaxFormatServiceFactory = () => ({
            format: () => Promise.reject(new Error("network down")),
        });

        const result = await runFormatDax(
            { expressions: ["[X] := 1"] },
            {
                factory,
                serverVersion: "1",
                resolveClientInfo: () => ({ name: "claude-ai", version: "1" }),
            },
        );

        expect(result.isError).toBe(true);
        expect(result.content[0]?.text).toContain("network down");
    });
});
