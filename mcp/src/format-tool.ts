import { z } from "zod";
import { DaxFormatterClient } from "./daxformatter/index";
import type { DaxFormatOptions, DaxFormatResult } from "./daxformatter/index";
import { CALLER_APP } from "./version";

export const FORMAT_DAX_TOOL_NAME = "format_dax";

const inputSchema = z.object({
    expressions: z
        .array(z.string().min(1))
        .min(1)
        .describe(
            "One or more DAX expressions to format. Several are formatted in a single request.",
        ),
    lineStyle: z
        .enum(["longLine", "shortLine"])
        .optional()
        .describe("Line-length style. Defaults to longLine."),
    spacingStyle: z
        .enum(["spaceAfterFunction", "noSpaceAfterFunction"])
        .optional()
        .describe("Spacing after a function name. Defaults to spaceAfterFunction."),
    listSeparator: z
        .string()
        .length(1)
        .optional()
        .describe("List separator character. Defaults to ','."),
    decimalSeparator: z
        .string()
        .length(1)
        .optional()
        .describe("Decimal separator character. Defaults to '.'."),
    serverName: z
        .string()
        .optional()
        .describe(
            "Name of the server the DAX expressions were taken from. Anonymous usage statistics only (does not affect formatting); this server SHA-256 hashes it before forwarding to daxformatter.com, so the statistics service only ever sees the hash. Pass it only if you can retrieve a real value (e.g. from the active connection or the model/project files); otherwise omit it. Never invent, guess, or hallucinate it.",
        ),
    databaseName: z
        .string()
        .optional()
        .describe(
            "Name of the database/model the DAX expressions were taken from. Anonymous usage statistics only (does not affect formatting); this server SHA-256 hashes it before forwarding to daxformatter.com, so the statistics service only ever sees the hash. Pass it only if you can retrieve a real value (e.g. from the active connection or the model/project files); otherwise omit it. Never invent, guess, or hallucinate it.",
        ),
});

export const formatDaxInputSchema = inputSchema.shape;
export type FormatDaxArgs = z.infer<typeof inputSchema>;

const outputSchema = z.object({
    results: z
        .array(
            z.object({
                formatted: z.string().nullable(),
                errors: z.array(
                    z.object({
                        line: z.number().nullable(),
                        column: z.number().nullable(),
                        message: z.string().nullable(),
                    }),
                ),
            }),
        )
        .describe("One result per input expression, in the same order."),
});

export const formatDaxOutputSchema = outputSchema.shape;

/** Structured payload returned by a successful {@link runFormatDax} call. */
export type FormatDaxOutput = z.infer<typeof outputSchema>;

/** Minimal surface of the DAX Formatter SDK the tool needs (injectable for tests). */
export interface DaxFormatService {
    format(expressions: string[], options: DaxFormatOptions): Promise<DaxFormatResult[]>;
}

export type DaxFormatServiceFactory = (appName: string, appVersion: string) => DaxFormatService;

/** Default factory: a real DaxFormatterClient stamped with the caller identity. */
export const defaultServiceFactory: DaxFormatServiceFactory = (appName, appVersion) => {
    const client = new DaxFormatterClient({ appName, appVersion });
    return { format: (expressions, options) => client.format(expressions, options) };
};

export interface FormatDaxDeps {
    factory: DaxFormatServiceFactory;
    /** Fallback callerVersion used when the client did not report its version. */
    serverVersion: string;
    /** The connected MCP client's identity (from `initialize`), used to build the caller id. */
    resolveClientInfo: () => { name?: string; version?: string } | undefined;
}

/** A single text content block, as carried by an MCP tool result. */
type TextContent = { type: "text"; text: string };

/**
 * Successful {@link runFormatDax} result: the structured formatter output plus a text rendering
 * of the same payload for clients that don't consume structured content.
 *
 * Declared as a `type` (not an `interface`) so it stays assignable to the MCP SDK's extensible
 * `CallToolResult`, which carries an index signature.
 */
export type FormatDaxResult = {
    content: TextContent[];
    structuredContent: FormatDaxOutput;
};

/** Generic tool failure: a human-readable message surfaced to the MCP client via `isError`. */
export type Problem = {
    isError: true;
    content: TextContent[];
};

function toOptions(args: FormatDaxArgs): DaxFormatOptions {
    const { expressions, ...options } = args;
    return options;
}

export async function runFormatDax(
    args: FormatDaxArgs,
    deps: FormatDaxDeps,
): Promise<FormatDaxResult | Problem> {
    // Caller id from the connected MCP client, "<this MCP>/<client>" — the client segment (and its
    // separator) is dropped when unavailable:
    //   callerApp     = "daxformatter-mcp-local/<client-name>"  (or "daxformatter-mcp-local")
    //   callerVersion = "<mcp-version>/<client-version>"        (or "<mcp-version>")
    const clientInfo = deps.resolveClientInfo();
    const appName = clientInfo?.name ? `${CALLER_APP}/${clientInfo.name}` : CALLER_APP;
    const appVersion = clientInfo?.version
        ? `${deps.serverVersion}/${clientInfo.version}`
        : deps.serverVersion;
    const service = deps.factory(appName, appVersion);

    try {
        const results = await service.format(args.expressions, toOptions(args));
        return {
            content: [{ type: "text", text: JSON.stringify({ results }, null, 2) }],
            structuredContent: { results },
        };
    } catch (error) {
        const message = error instanceof Error ? error.message : String(error);
        return {
            isError: true,
            content: [{ type: "text", text: `DAX Formatter request failed: ${message}` }],
        };
    }
}
