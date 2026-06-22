import { describe, expect, it } from "vitest";
import { sha256Hex } from "../../src/daxformatter/hash";

describe("sha256Hex", () => {
    // Same vectors as the .NET SDK's SecurityHelpersTests, so both clients are
    // guaranteed to produce identical hashes for the same input.
    it.each([
        ["MyValue123456789$", "5f45bfb8ab5c9ea6fe0762974f7bbbe3602155c66d1139496fc2246c360a874b"],
        ["1234567890??=", "cb0f4399a2850ee589414b82de85b736ced269035c48e4275be850a3162ba284"],
        [
            "abcdefghiABCDEFGHI??=",
            "1bca6736f96f84e35fa921938f45ba981a6e3f6aa02bcf46763009d3614cf89d",
        ],
        ["", "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"],
    ])("hashes %j to match the .NET SDK", (value, expected) => {
        expect(sha256Hex(value)).toBe(expected);
    });

    // The .NET helper maps null -> null; in TypeScript that is handled one level up
    // (null/undefined metadata is omitted before hashing), so sha256Hex only ever
    // receives a string.
});
