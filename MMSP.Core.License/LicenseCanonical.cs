using System.Text;

namespace MMSP.Core.License
{
    /// <summary>
    /// 签名覆盖的 8 行文本。行之间是 LF，第 8 行后面还有一个 LF。
    /// </summary>
    public static class LicenseCanonical
    {
        /// <summary>
        /// 拼出规范字节。模块按给定顺序连接，不排序。空名单时第 8 行是空行。
        /// </summary>
        public static byte[] Build(
            int format,
            Guid licenseId,
            string customerCode,
            string machineFingerprint,
            DateTimeOffset notBeforeUtc,
            DateTimeOffset notAfterUtc,
            DateTimeOffset issuedAtUtc,
            IReadOnlyList<string> modules)
        {
            ArgumentNullException.ThrowIfNull(modules);
            var modulesLine = modules.Count == 0 ? "" : string.Join(",", modules);
            var text = string.Join(
                "\n",
                format.ToString(System.Globalization.CultureInfo.InvariantCulture),
                licenseId.ToString("D"),
                customerCode,
                machineFingerprint,
                notBeforeUtc.ToString("O"),
                notAfterUtc.ToString("O"),
                issuedAtUtc.ToString("O"),
                modulesLine) + "\n";
            return Encoding.UTF8.GetBytes(text);
        }
    }
}
