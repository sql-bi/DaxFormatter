import { describe, expect, it, vi } from "vitest";
import { DaxFormatterClient } from "../../src/daxformatter/client";

function jsonResponse(body: unknown): Response {
    return new Response(JSON.stringify(body), {
        status: 200,
        headers: { "content-type": "application/json" },
    });
}

describe("DaxFormatterClient", () => {
    it("posts to the multi endpoint with the expected body", async () => {
        const fetchMock = vi.fn(async () => jsonResponse([{ formatted: "A\r\n", errors: [] }]));
        const client = new DaxFormatterClient({
            fetch: fetchMock as unknown as typeof fetch,
            appName: "test",
            appVersion: "1",
        });

        const results = await client.format(["a", "b"], { serverName: "MyServer" });

        expect(fetchMock).toHaveBeenCalledOnce();
        const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
        expect(url).toBe("https://api.daxformatter.com/api/daxtextformatmulti");
        expect(init.method).toBe("POST");

        const body = JSON.parse(init.body as string);
        expect(body.dax).toEqual(["a", "b"]);
        expect(body.serverName).toBe(
            "5e3e900585e7834b034d4f17187e521b3c7b7de0d1637f79de4b63717d4f0783",
        );
        expect(body.callerApp).toBe("test");

        expect(results).toEqual([{ formatted: "A\r\n", errors: [] }]);
    });

    it("posts a single string to the single endpoint and normalizes a missing errors list", async () => {
        const fetchMock = vi.fn(async () => jsonResponse({ formatted: "EVALUATE\r\n" }));
        const client = new DaxFormatterClient({ fetch: fetchMock as unknown as typeof fetch });

        const result = await client.format("evaluate('Table')");

        const [url] = fetchMock.mock.calls[0] as [string];
        expect(url).toBe("https://api.daxformatter.com/api/daxtextformat");
        expect(result).toEqual({ formatted: "EVALUATE\r\n", errors: [] });
    });

    it("throws on a non-OK response", async () => {
        const fetchMock = vi.fn(
            async () => new Response("nope", { status: 500, statusText: "Server Error" }),
        );
        const client = new DaxFormatterClient({ fetch: fetchMock as unknown as typeof fetch });

        await expect(client.format(["a"])).rejects.toThrow(/500/);
    });
});
