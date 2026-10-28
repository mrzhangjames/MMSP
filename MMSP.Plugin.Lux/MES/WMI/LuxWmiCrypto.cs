using System.Security.Cryptography;
using System.Text;
using MMSP.Common.Helper.Logger;

namespace MMSP.Plugin.Lux.WMI
{
    /// <summary>
    /// WMI 报文的 AES-256-ECB-PKCS7。密钥不是 32 字节就失败，不补齐。
    /// </summary>
    public static class LuxWmiCrypto
    {
        /// <summary>
        /// 加密并做成 Base64。密钥长度不对时返回 false，不改输出。
        /// </summary>
        public static bool TryEncrypt(string plain, string? key, out string cipher, out string error)
        {
            cipher = "";
            error = "";
            if (!TryKey(key, out var keyBytes, out error))
                return false;

            var data = Encoding.UTF8.GetBytes(plain ?? "");
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            cipher = Convert.ToBase64String(aes.CreateEncryptor().TransformFinalBlock(data, 0, data.Length));
            return true;
        }

        /// <summary>
        /// 解密 Base64 密文。只给校验使用。
        /// </summary>
        public static bool TryDecrypt(string cipher, string? key, out string plain, out string error)
        {
            plain = "";
            error = "";
            if (!TryKey(key, out var keyBytes, out error))
                return false;

            try
            {
                var data = Convert.FromBase64String(cipher ?? "");
                using var aes = Aes.Create();
                aes.Key = keyBytes;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                var raw = aes.CreateDecryptor().TransformFinalBlock(data, 0, data.Length);
                plain = Encoding.UTF8.GetString(raw);
                return true;
            }
            catch (FormatException)
            {
                // 密文本身不写入日志。
                LogHelper.MesIoError("AES 密文无法解密");
                error = "AES 密文无法解密";
                return false;
            }
            catch (CryptographicException)
            {
                LogHelper.MesIoError("AES 密文无法解密");
                error = "AES 密文无法解密";
                return false;
            }
        }

        /// <summary>
        /// 密钥按 UTF-8 必须正好 32 字节。
        /// </summary>
        private static bool TryKey(string? key, out byte[] keyBytes, out string error)
        {
            keyBytes = Encoding.UTF8.GetBytes(key ?? "");
            if (keyBytes.Length == 32)
            {
                error = "";
                return true;
            }

            error = "AES 密钥长度不正确";
            return false;
        }
    }
}
