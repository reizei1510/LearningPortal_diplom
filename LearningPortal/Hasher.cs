using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace LearningPortal
{
    public class Hasher
    {
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            return Convert.ToBase64String(saltBytes);
        }

        public static string Hash(string str, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            string hashedStr = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: str,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            return hashedStr;
        }

        public static bool Verify(string str, string hashedStr, string salt)
        {
            string hashedEnteredStr = Hash(str, salt);

            return hashedEnteredStr == hashedStr;
        }
    }
}