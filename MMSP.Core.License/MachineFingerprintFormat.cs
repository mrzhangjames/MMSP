using System.Security.Cryptography;
using System.Text;

namespace MMSP.Core.License
{
    /// <summary>
    /// 机器指纹的文本规则。不读注册表，注册表由调用方读取后把原文传进来。
    /// </summary>
    public static class MachineFingerprintFormat
    {
        /// <summary>
        /// 通用证第 4 行。全小写，没有别的写法。
        /// </summary>
        public const string Universal = "universal";

        /// <summary>
        /// 参与哈希的前缀。后面接 Guid 的 D 格式小写。
        /// </summary>
        public const string Prefix = "mmsp-machine-v1|";

        /// <summary>
        /// 是不是通用证指纹。
        /// </summary>
        public static bool IsUniversal(string? value) => value == Universal;

        /// <summary>
        /// 是不是 64 位小写十六进制。
        /// </summary>
        public static bool IsMachineHex(string? value)
        {
            if (value is not { Length: 64 })
                return false;

            foreach (var c in value)
            {
                var digit = c is >= '0' and <= '9';
                var lower = c is >= 'a' and <= 'f';
                if (!digit && !lower)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 把 MachineGuid 原文收成指纹。空串、去花括号后仍不是 Guid，都算读不到。
        /// </summary>
        public static bool TryFromMachineGuid(string? raw, out string fingerprint)
        {
            fingerprint = "";
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            var text = raw.Trim();
            if (text.Length >= 2 && text[0] == '{' && text[^1] == '}')
                text = text[1..^1];

            if (!Guid.TryParse(text, out var guid))
                return false;

            fingerprint = FromGuid(guid);
            return true;
        }

        /// <summary>
        /// 用规范化 Guid 计算指纹。同一 Guid 的大小写和花括号不会改变结果。
        /// </summary>
        public static string FromGuid(Guid guid)
        {
            var text = Prefix + guid.ToString("D");
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
