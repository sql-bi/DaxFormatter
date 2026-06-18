/**
 * Public types for the DAX Formatter client.
 *
 * `DaxLineStyle` and `DaxSpacingStyle` are friendly string unions translated to
 * their wire form in `serialize.ts` (`lineStyle` → number, spacing → boolean).
 * The server* metadata are string enums whose value IS the exact wire string the
 * service expects, so they need no translation; member names mirror the .NET SDK.
 */

/** Maximum line length style. Wire: `longLine` → 0, `shortLine` → 1. */
export type DaxLineStyle = "longLine" | "shortLine";

/**
 * Spacing after a function name. Wire: a boolean `skipSpaceAfterFunctionName`
 * (`spaceAfterFunction` → false, `noSpaceAfterFunction` → true). The service
 * default ("best practice") is `spaceAfterFunction`.
 */
export type DaxSpacingStyle = "spaceAfterFunction" | "noSpaceAfterFunction";

/**
 * Analysis Services server type. The enum value is the exact wire string the
 * service expects; member names mirror the .NET SDK's `ServerType` enum.
 */
export enum ServerType {
    AnalysisServices = "SSAS",
    PowerBIDesktop = "PBI Desktop",
    PowerBIReportServer = "PBI Report Server",
    PowerPivot = "PowerPivot",
    SSDT = "SSDT",
    AzureAnalysisServices = "AzureAS",
    PowerBIService = "PBI Service",
    Offline = "Offline",
}

/** Analysis Services server mode. Value = wire string; names mirror the .NET SDK. */
export enum ServerMode {
    Multidimensional = "Multidimensional",
    SharePoint = "SharePoint",
    Tabular = "Tabular",
    Default = "Default",
}

/** Analysis Services server edition. Value = wire string; names mirror the .NET SDK. */
export enum ServerEdition {
    Standard = "Standard",
    Standard64 = "Standard64",
    Enterprise = "Enterprise",
    Enterprise64 = "Enterprise64",
    Developer = "Developer",
    Developer64 = "Developer64",
    Evaluation = "Evaluation",
    Evaluation64 = "Evaluation64",
    LocalCube = "LocalCube",
    LocalCube64 = "LocalCube64",
    BusinessIntelligence = "BusinessIntelligence",
    BusinessIntelligence64 = "BusinessIntelligence64",
    EnterpriseCore = "EnterpriseCore",
    EnterpriseCore64 = "EnterpriseCore64",
}

/** Server location. Value = wire string; names mirror the .NET SDK. */
export enum ServerLocation {
    OnPremise = "OnPremise",
    Azure = "Azure",
}

/**
 * Formatting options and the optional model metadata.
 *
 * Provide as much of the model metadata as you can: the service uses it for
 * anonymous usage statistics. `serverName` and `databaseName` are SHA-256
 * hashed by the client before being sent — they never leave in clear text.
 */
export interface DaxFormatOptions {
    /** Default: `longLine`. */
    lineStyle?: DaxLineStyle;
    /** Default: `spaceAfterFunction`. */
    spacingStyle?: DaxSpacingStyle;
    /** Single character. Default: `,`. */
    listSeparator?: string;
    /** Single character. Default: `.`. */
    decimalSeparator?: string;

    /** Hashed (SHA-256) before sending. */
    serverName?: string;
    /** Hashed (SHA-256) before sending. */
    databaseName?: string;
    serverType?: ServerType;
    serverMode?: ServerMode;
    serverEdition?: ServerEdition;
    serverLocation?: ServerLocation;
    /** Example: `14.0.800.192`. */
    serverVersion?: string;
    databaseCompatibilityLevel?: string;
}

/** A formatting error reported by the service, with its position in the input. */
export interface DaxFormatError {
    line: number | null;
    column: number | null;
    message: string | null;
}

/** The result of formatting a single DAX expression. */
export interface DaxFormatResult {
    /** The formatted expression, or `null` when the input could not be formatted. */
    formatted: string | null;
    /** Syntax errors found in the input (empty when the expression is valid). */
    errors: DaxFormatError[];
}
