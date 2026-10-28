using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using MMSP.Core.Model.Helper;

namespace MMSP.Service.Database
{
    /// <summary>
    /// 把口令写成 <c>pbkdf2$迭代$盐$哈希</c>，登录只认这一列。不保存明文，也不按明文比较。
    /// </summary>
    public static class UserPassword
    {
        /// <summary>
        /// 新口令的迭代次数。
        /// </summary>
        public const int DefaultIterations = 600_000;

        /// <summary>
        /// 低于这个次数的记录直接拒绝，不再计算。
        /// </summary>
        public const int MinimumIterations = 100_000;

        /// <summary>
        /// 盐的字节数。
        /// </summary>
        private const int SaltLength = 16;

        /// <summary>
        /// 哈希的字节数。
        /// </summary>
        private const int HashLength = 32;

        /// <summary>
        /// 列里的算法名。
        /// </summary>
        private const string Scheme = "pbkdf2";

        /// <summary>
        /// 生成一列可入库的口令。同一口令每次盐不同。
        /// </summary>
        public static string Hash(string password, int iterations = DefaultIterations)
        {
            if (iterations < MinimumIterations)
                throw new ArgumentOutOfRangeException(nameof(iterations));

            var salt = new byte[SaltLength];
            RandomNumberGenerator.Fill(salt);
            var hash = Derive(password ?? "", salt, iterations);
            return string.Join(
                '$',
                Scheme,
                iterations.ToString(CultureInfo.InvariantCulture),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        /// <summary>
        /// 库存是否已经是合法哈希。明文、空值和损坏的记录都返回 false。
        /// </summary>
        public static bool IsHashed(string? stored)
        {
            return TryRead(stored, out _, out _, out _);
        }

        /// <summary>
        /// 用库存哈希校验口令。格式不对或口令不对都返回 false，不抛异常。
        /// </summary>
        public static bool Verify(string? stored, string password)
        {
            if (!TryRead(stored, out var iterations, out var salt, out var expected))
                return false;

            var actual = Derive(password ?? "", salt, iterations);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        /// <summary>
        /// 拆开四段。算法名、迭代、盐长度或哈希长度不对就拒绝。
        /// </summary>
        private static bool TryRead(string? stored, out int iterations, out byte[] salt, out byte[] expected)
        {
            iterations = 0;
            salt = [];
            expected = [];
            if (string.IsNullOrEmpty(stored))
                return false;

            var parts = stored.Split('$');
            if (parts.Length != 4 || !string.Equals(parts[0], Scheme, StringComparison.Ordinal))
                return false;

            if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out iterations)
                || iterations < MinimumIterations)
                return false;

            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expected = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                // 原文里可能有盐或哈希，日志不附带这个异常。
                CaughtLog.Report(CaughtLogChannel.Sys, "口令记录无法解析", new FormatException("口令记录无法解析"));
                return false;
            }

            return salt.Length == SaltLength && expected.Length == HashLength;
        }

        /// <summary>
        /// UTF-8 口令做 PBKDF2-SHA256。
        /// </summary>
        private static byte[] Derive(string password, byte[] salt, int iterations)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                HashLength);
        }
    }
}
