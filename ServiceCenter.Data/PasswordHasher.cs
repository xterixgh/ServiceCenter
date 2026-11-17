using System.Security.Cryptography;
using System.Text;

namespace ServiceCenter.Data
{
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        public static bool Verify(string password, string hash)
        {
            return Hash(password) == hash;
        }
    }
}
