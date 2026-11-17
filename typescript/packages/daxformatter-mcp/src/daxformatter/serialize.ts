import { sha256Hex } from "./hash";
import type { DaxFormatOptions, DaxLineStyle, DaxSpacingStyle } from "./types";

const lineStyleWire: Record<DaxLineStyle, number> = {
    longLine: 0,
    shortLine: 1,
};

const spacingWire: Record<DaxSpacingStyle, boolean> = {
    spaceAfterFunction: false,
    noSpaceAfterFunction: true,
};

export interface CallerInfo {
    callerApp?: string;
    callerVersion?: string;
}

/**
 * Builds the wire payload shared by single and multiple requests (everything
 * except the `dax` field). Mirrors the .NET SDK's request serialization:
 * defaults are always sent, null/undefined metadata is omitted, `serverName`
 * and `databaseName` are SHA-256 hashed, and the enums use the exact wire
 * representation the service expects.
 */
export function buildRequestPayload(
    options: DaxFormatOptions,
    caller: CallerInfo,
): Record<string, unknown> {
    const payload: Record<string, unknown> = {
        maxLineLength: lineStyleWire[options.lineStyle ?? "longLine"],
        skipSpaceAfterFunctionName: spacingWire[options.spacingStyle ?? "spaceAfterFunction"],
        listSeparator: options.listSeparator ?? ",",
        decimalSeparator: options.decimalSeparator ?? ".",
    };

    if (options.serverName != null) payload.serverName = sha256Hex(options.serverName);
    if (options.databaseName != null) payload.databaseName = sha256Hex(options.databaseName);
    if (options.serverType != null) payload.serverType = options.serverType;
    if (options.serverMode != null) payload.serverMode = options.serverMode;
    if (options.serverEdition != null) payload.serverEdition = options.serverEdition;
    if (options.serverLocation != null) payload.serverLocation = options.serverLocation;
    if (options.serverVersion != null) payload.serverVersion = options.serverVersion;
    if (options.databaseCompatibilityLevel != null)
        payload.databaseCompatibilityLevel = options.databaseCompatibilityLevel;
    if (caller.callerApp != null) payload.callerApp = caller.callerApp;
    if (caller.callerVersion != null) payload.callerVersion = caller.callerVersion;

    return payload;
}
