using Scrypt;

namespace AuthService.Extensions
{
    public static class StringHashExtension
    {
        public static string Hash(this string source)
        {
            return new ScryptEncoder().Encode(source);
        }

        public static bool Verify(this string source, string hash)
        {
            return new ScryptEncoder().Compare(source, hash);
        }
    }
}
