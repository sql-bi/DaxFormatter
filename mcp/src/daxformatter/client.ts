import { type CallerInfo, buildRequestPayload } from "./serialize";
import type { DaxFormatOptions, DaxFormatResult } from "./types";

const BASE_URL = "https://api.daxformatter.com";
const SINGLE_PATH = "/api/daxtextformat";
const MULTI_PATH = "/api/daxtextformatmulti";
const DEFAULT_TIMEOUT_MS = 60_000;

export interface DaxFormatterClientOptions {
    /** Your application's name, recorded by the service for anonymous usage statistics. */
    appName?: string;
    /** Your application's version, recorded by the service alongside {@link appName}. */
    appVersion?: string;
    /** Per-request timeout in milliseconds. Defaults to 60000. */
    timeoutMs?: number;
    /** Override the `fetch` implementation (defaults to the global `fetch`). */
    fetch?: typeof fetch;
}

/**
 * Client for the DAX Formatter service. Reuse a single instance across requests.
 *
 * The service formats DAX server-side; this client only handles transport,
 * serialization, and hashing of the model metadata. Syntax errors are returned
 * in {@link DaxFormatResult.errors} with their line/column — the same call both
 * formats and validates.
 */
export class DaxFormatterClient {
    private readonly caller: CallerInfo;
    private readonly timeoutMs: number;
    private readonly fetchImpl: typeof fetch;

    constructor(options: DaxFormatterClientOptions = {}) {
        this.caller = { callerApp: options.appName, callerVersion: options.appVersion };
        this.timeoutMs = options.timeoutMs ?? DEFAULT_TIMEOUT_MS;
        this.fetchImpl = options.fetch ?? globalThis.fetch;

        if (typeof this.fetchImpl !== "function") {
            throw new Error(
                "No fetch implementation available. Provide one via options.fetch or run on Node 18+.",
            );
        }
    }

    /** Formats a single DAX expression. */
    async format(expression: string, options?: DaxFormatOptions): Promise<DaxFormatResult>;
    /** Formats multiple DAX expressions in a single request. */
    async format(expressions: string[], options?: DaxFormatOptions): Promise<DaxFormatResult[]>;
    async format(
        input: string | string[],
        options: DaxFormatOptions = {},
    ): Promise<DaxFormatResult | DaxFormatResult[]> {
        if (Array.isArray(input)) {
            const payload = { dax: input, ...buildRequestPayload(options, this.caller) };
            const results = await this.post<DaxFormatResult[] | null>(MULTI_PATH, payload);
            return (results ?? []).map(normalizeResult);
        }

        const payload = { dax: input, ...buildRequestPayload(options, this.caller) };
        const result = await this.post<DaxFormatResult | null>(SINGLE_PATH, payload);
        return normalizeResult(result);
    }

    private async post<T>(path: string, body: unknown): Promise<T> {
        const controller = new AbortController();
        const timer = setTimeout(() => controller.abort(), this.timeoutMs);

        try {
            const response = await this.fetchImpl(`${BASE_URL}${path}`, {
                method: "POST",
                headers: { "content-type": "application/json", accept: "application/json" },
                body: JSON.stringify(body),
                signal: controller.signal,
            });

            if (!response.ok) {
                throw new Error(
                    `DAX Formatter request failed: ${response.status} ${response.statusText}`,
                );
            }

            return (await response.json()) as T;
        } finally {
            clearTimeout(timer);
        }
    }
}

function normalizeResult(result: DaxFormatResult | null | undefined): DaxFormatResult {
    return { formatted: result?.formatted ?? null, errors: result?.errors ?? [] };
}
