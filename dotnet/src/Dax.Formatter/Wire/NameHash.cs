using System.Security.Cryptography;
using System.Text;

namespace Dax.Formatter.Wire;

/// <summary>
/// The hash that replaces the name of a server or of a database in a request, so that the service can count
/// distinct names without learning them.
/// </summary>
internal static class NameHash
{
    private const string HexDigits = "0123456789abcdef";

    public static string Compute(string name)
    {
        var hash = ComputeSha256(Encoding.UTF8.GetBytes(name));

        var hex = new char[hash.Length * 2];
        for (var i = 0; i < hash.Length; i++)
        {
            hex[i * 2] = HexDigits[hash[i] >> 4];
            hex[(i * 2) + 1] = HexDigits[hash[i] & 0xF];
        }

        return new string(hex);
    }

    private static byte[] ComputeSha256(byte[] bytes)
    {
#if NET
        return SHA256.HashData(bytes);
#else
        using var algorithm = SHA256.Create();
        return algorithm.ComputeHash(bytes);
#endif
    }
}
