namespace Dax.Formatter.Security.Cryptography
{
    using System.Security.Cryptography;

    internal static class DataProtection
    {
        public static byte[] Protect(byte[] userData) => ProtectedData.Protect(userData, optionalEntropy: null, DataProtectionScope.CurrentUser);

        public static byte[] Unprotect(byte[] encryptedData) => ProtectedData.Unprotect(encryptedData, optionalEntropy: null, DataProtectionScope.CurrentUser);
    }
}
