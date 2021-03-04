namespace Dax.Formatter.Security
{
    using System.Security.Cryptography;
    using System.Text;

    internal static class SecurityHelpers
    {
        public static string ToHashSHA256(this string value)
        {
            if (value == null) 
                return null;

            var stringBuilder = new StringBuilder();

            var buffer = Encoding.UTF8.GetBytes(value);
            var offset = 0;
            var count = Encoding.UTF8.GetByteCount(value);

            using (var algorithm = new SHA256Managed())
            {
                var bytes = algorithm.ComputeHash(buffer, offset, count);

                foreach (var @byte in bytes)
                    stringBuilder.Append(@byte.ToString("x2"));
            }

            return stringBuilder.ToString();
        }
    }
}
