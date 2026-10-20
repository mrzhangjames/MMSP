using System.IO;
using System.Security;
using Microsoft.Win32;
using MMSP.Core.License;

namespace MMSP.App.WpfBase.Services
{
    /// <summary>
    /// 从 64 位注册表视图读取 MachineGuid，并收成许可证指纹。
    /// </summary>
    public static class MachineFingerprintReader
    {
        /// <summary>
        /// MachineGuid 所在的键。固定走 Registry64，避免 32 位视图读到 WOW6432Node。
        /// </summary>
        private const string CryptographyKey = @"SOFTWARE\Microsoft\Cryptography";

        /// <summary>
        /// 读到并规范化时返回 true。读不到时 <paramref name="fingerprint"/> 是空串。
        /// </summary>
        public static bool TryRead(out string fingerprint)
        {
            fingerprint = "";
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using var crypto = baseKey.OpenSubKey(CryptographyKey);
                var raw = crypto?.GetValue("MachineGuid") as string;
                return MachineFingerprintFormat.TryFromMachineGuid(raw, out fingerprint);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException or InvalidOperationException)
            {
                return false;
            }
        }
    }
}
