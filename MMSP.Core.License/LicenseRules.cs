using System.Text.RegularExpressions;

namespace MMSP.Core.License
{
    /// <summary>
    /// 许可证字段的格式。不符合就不要进入验签。
    /// </summary>
    internal static partial class LicenseRules
    {
        /// <summary>
        /// 客户编码：1 到 64 个可见 ASCII。
        /// </summary>
        [GeneratedRegex("^[\\x21-\\x7E]{1,64}$")]
        private static partial Regex CustomerPattern();

        /// <summary>
        /// 模块名：MMSP.App 或 MMSP.Plugin，再加一段字母数字。
        /// </summary>
        [GeneratedRegex("^MMSP\\.(App|Plugin)\\.[A-Za-z0-9]+$")]
        private static partial Regex ModulePattern();

        /// <summary>
        /// 客户编码是否可写入规范行。
        /// </summary>
        public static bool IsCustomer(string? value) => value is not null && CustomerPattern().IsMatch(value);

        /// <summary>
        /// 模块名是否可写入名单。
        /// </summary>
        public static bool IsModule(string? value) => value is not null && ModulePattern().IsMatch(value);

        /// <summary>
        /// 指纹只允许 universal 或 64 位小写十六进制。
        /// </summary>
        public static bool IsFingerprint(string? value) =>
            MachineFingerprintFormat.IsUniversal(value) || MachineFingerprintFormat.IsMachineHex(value);
    }
}
