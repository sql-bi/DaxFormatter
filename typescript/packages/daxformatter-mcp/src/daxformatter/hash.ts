import { createHash } from "node:crypto";

/**
 * SHA-256 of the UTF-8 bytes of `value`, lowercase hex.
 *
 * Must stay byte-for-byte identical to the .NET SDK's
 * `SecurityHelpers.ToHashSHA256`, so that server-side statistics correlate
 * across the .NET and TypeScript clients.
 */
export function sha256Hex(value: string): string {
    return createHash("sha256").update(value, "utf8").digest("hex");
}
