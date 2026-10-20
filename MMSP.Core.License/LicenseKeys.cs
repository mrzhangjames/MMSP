using System.Security.Cryptography;

namespace MMSP.Core.License
{
    /// <summary>
    /// 生成 RSA-2048 密钥对。私钥是 PKCS#8，公钥是 SubjectPublicKeyInfo。
    /// </summary>
    public static class LicenseKeys
    {
        /// <summary>
        /// 生成一对新密钥。调用方负责把私钥存到仓库外面。
        /// </summary>
        public static (byte[] PrivatePkcs8, byte[] PublicSpki) Generate()
        {
            using var rsa = RSA.Create(2048);
            return (rsa.ExportPkcs8PrivateKey(), rsa.ExportSubjectPublicKeyInfo());
        }

        /// <summary>
        /// 从私钥导出公钥。签发和写入主程序用同一把。
        /// </summary>
        public static byte[] PublicFromPrivate(ReadOnlySpan<byte> pkcs8PrivateKey)
        {
            if (pkcs8PrivateKey.IsEmpty)
                throw new ArgumentException("私钥为空。", nameof(pkcs8PrivateKey));

            using var rsa = RSA.Create();
            try
            {
                rsa.ImportPkcs8PrivateKey(pkcs8PrivateKey, out _);
            }
            catch (CryptographicException ex)
            {
                throw new ArgumentException("私钥无效。", nameof(pkcs8PrivateKey), ex);
            }

            return rsa.ExportSubjectPublicKeyInfo();
        }
    }
}
