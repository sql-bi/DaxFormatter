import { describe, expect, it } from "vitest";
import { buildRequestPayload } from "../../src/daxformatter/serialize";
import {
    ServerEdition,
    ServerLocation,
    ServerMode,
    ServerType,
} from "../../src/daxformatter/types";

describe("buildRequestPayload", () => {
    // Cross-checked against the JSON produced by the .NET SDK, except for serverMode,
    // serverEdition and serverLocation: the .NET SDK has no string-enum converter on those
    // properties and emits their numeric ordinal, but the REST API expects the enum-member
    // string (e.g. "OnPremise"), so this client sends the string the service documents.
    it("produces the minimal payload with defaults", () => {
        expect(buildRequestPayload({}, {})).toEqual({
            maxLineLength: 0,
            skipSpaceAfterFunctionName: false,
            listSeparator: ",",
            decimalSeparator: ".",
        });
    });

    it("maps every option to the exact wire representation", () => {
        const payload = buildRequestPayload(
            {
                lineStyle: "shortLine",
                spacingStyle: "noSpaceAfterFunction",
                listSeparator: ";",
                decimalSeparator: ",",
                serverName: "MyServer",
                databaseName: "MyDatabase",
                serverVersion: "14.0.800.192",
                databaseCompatibilityLevel: "1500",
                serverType: ServerType.AzureAnalysisServices,
                serverMode: ServerMode.Tabular,
                serverEdition: ServerEdition.Enterprise,
                serverLocation: ServerLocation.OnPremise,
            },
            { callerApp: "daxformatter-mcp", callerVersion: "1.0.0" },
        );

        expect(payload).toEqual({
            serverName: "5e3e900585e7834b034d4f17187e521b3c7b7de0d1637f79de4b63717d4f0783",
            databaseName: "269b0b97f05c31932ebbb1e04800beb3a2114eb48fa650f9c77d4bdebf22f542",
            serverEdition: "Enterprise",
            serverType: "AzureAS",
            serverMode: "Tabular",
            serverLocation: "OnPremise",
            serverVersion: "14.0.800.192",
            databaseCompatibilityLevel: "1500",
            maxLineLength: 1,
            skipSpaceAfterFunctionName: true,
            listSeparator: ";",
            decimalSeparator: ",",
            callerApp: "daxformatter-mcp",
            callerVersion: "1.0.0",
        });
    });

    it("omits null/undefined metadata", () => {
        const payload = buildRequestPayload({ serverName: "MyServer" }, {});
        expect(payload).not.toHaveProperty("databaseName");
        expect(payload).not.toHaveProperty("serverType");
        expect(payload).not.toHaveProperty("callerApp");
        expect(payload.serverName).toBe(
            "5e3e900585e7834b034d4f17187e521b3c7b7de0d1637f79de4b63717d4f0783",
        );
    });
});
