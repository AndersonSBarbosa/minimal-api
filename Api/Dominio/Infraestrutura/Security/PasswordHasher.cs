using System;
using System.Security.Cryptography;

namespace minimal_api.Dominio.Infraestrutura.Security
{
    internal static class PasswordHasher
    {
        // Format: {iterations}.{saltBase64}.{subkeyBase64}
        public static string HashPassword(string password)
        {
            if (password is null) throw new ArgumentNullException(nameof(password));

            const int iterations = 100_000;
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] subkey = pbkdf2.GetBytes(32);

            return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(subkey)}";
        }

        public static bool VerifyPassword(string hashedPassword, string password)
        {
            if (hashedPassword is null) throw new ArgumentNullException(nameof(hashedPassword));
            if (password is null) throw new ArgumentNullException(nameof(password));

            var parts = hashedPassword.Split('.');
            if (parts.Length != 3) return false;

            if (!int.TryParse(parts[0], out int iterations)) return false;
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] expectedSubkey = Convert.FromBase64String(parts[2]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] actualSubkey = pbkdf2.GetBytes(expectedSubkey.Length);

            return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
        }
    }
}