using Scrypt;

namespace AuthService.Extensions
{
    public static class PasswordExtension
    {
        public static string Hash(this string password)
        {
            return new ScryptEncoder().Encode(password);
        }

        public static bool Verify(this string password, string hash)
        {
            return new ScryptEncoder().Compare(password, hash);
        }
    }
}
