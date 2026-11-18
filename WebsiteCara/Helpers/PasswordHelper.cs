using System;
using System.Security.Cryptography;
using System.Text;

namespace WebsiteCara.Helpers
{
    // Băm mật khẩu bằng PBKDF2-SHA256 (có sẵn trong .NET Framework, không cần cài thêm gói).
    // Định dạng lưu trong cột MatKhau:  PBKDF2$<số vòng lặp>$<salt base64>$<hash base64>
    public static class PasswordHelper
    {
        private const string Prefix = "PBKDF2";
        private const int Iterations = 100000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string Hash(string password)
        {
            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            var hash = Derive(password, salt, Iterations);
            return Prefix + "$" + Iterations + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
        }

        public static bool IsHashed(string stored)
        {
            return stored != null && stored.StartsWith(Prefix + "$", StringComparison.Ordinal);
        }

        // Tương thích tài khoản cũ: nếu mật khẩu trong DB chưa băm (chữ thường) thì so sánh trực tiếp.
        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored) || password == null) return false;

            if (!IsHashed(stored))
            {
                return ConstantEquals(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(stored));
            }

            var parts = stored.Split('$');
            if (parts.Length != 4) return false;

            int iterations;
            if (!int.TryParse(parts[1], out iterations) || iterations < 1) return false;

            byte[] salt, expected;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expected = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            return ConstantEquals(Derive(password, salt, iterations), expected);
        }

        private static byte[] Derive(string password, byte[] salt, int iterations)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(HashSize);
            }
        }

        private static bool ConstantEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
