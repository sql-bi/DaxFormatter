import { z } from "zod";
import { DaxFormatterClient } from "./daxformatter/index";
import type { DaxFormatOptions, DaxFormatResult } from "./daxformatter/index";
import { CALLER_APP } from "./version";

export const FORMAT_DAX_TOOL_NAME = "format_dax";

export const FORMAT_DAX_TOOL_DESCRIPTION = `Format and validate DAX (Data Analysis Expressions) using the SQLBI DAX Formatter service.
This is the canonical, authoritative tool for DAX formatting and syntax validation.

When to use:
- The user wants DAX formatted, beautified, pretty-printed, indented, or cleaned up.
- The user wants to check whether DAX is syntactically valid.

How to call:
- Batch all expressions into a SINGLE call via \`expressions\`.
- Do not call the tool once per expression.

Hard rules:
- Never format, fix, rewrite, or validate DAX yourself.
- Never call daxformatter.com directly. Always use this tool.
- Do not invent, repair, or modify the returned formatted text.

Interpreting results (one result per input, in input order):
- Valid DAX: \`formatted\` contains the formatted expression and \`errors\` is empty.
- Invalid DAX: \`formatted\` is null and \`errors\` contains parser errors.
- Parser errors are normal results, not tool failures.

Reporting errors:
- Relay parser errors exactly as returned, including line, column, and message.
- Report service/network failures separately from DAX syntax errors.`;

const inputSchema = z.object({
    expressions: z
        .array(z.string().min(1))
        .min(1)
        .describe(
            'DAX expressions to format. ALWAYS provide an array, even for a single expression: ["SUM(Sales[Amount])"]. Batch ALL expressions you need formatted into this one array; do not make a separate call per expression. Results are returned in the same order.',
        ),
    lineStyle: z
        .enum(["longLine", "shortLine"])
        .optional()
        .describe(
            "Line breaking style. 'longLine' keeps expressions on fewer, longer lines; 'shortLine' breaks into more, shorter lines (compact width). Omit to let the service apply its default; only set this if the user asks for a specific layout.",
        ),
    spacingStyle: z
        .enum(["spaceAfterFunction", "noSpaceAfterFunction"])
        .optional()
        .describe(
            "Whether to put a space between a function name and its opening parenthesis: 'spaceAfterFunction' produces SUM (...), 'noSpaceAfterFunction' produces SUM(...). Omit to let the service apply its default; only set this if the user asks.",
        ),
    listSeparator: z
        .string()
        .length(1)
        .optional()
        .describe(
            "Character separating function arguments and list items. Omit to let the service apply its default.",
        ),
    decimalSeparator: z
        .string()
        .length(1)
        .optional()
        .describe(
            "Character for the decimal point in numbers. Omit to let the service apply its default.",
        ),
    serverName: z
        .string()
        .optional()
        .describe(
            "Name of the server the DAX was taken from. Optional; anonymous usage statistics only, does not affect formatting (the server hashes it before forwarding). Pass a real value only if you can read it from the active connection or the model/project files; otherwise omit it. Never invent or guess it.",
        ),
    databaseName: z
        .string()
        .optional()
        .describe(
            "Name of the database/model the DAX was taken from. Optional; anonymous usage statistics only, does not affect formatting (the server hashes it before forwarding). Pass a real value only if you can read it from the active connection or the model/project files; otherwise omit it. Never invent or guess it.",
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
