using System.Text;

namespace MMSP.Core.License
{
    /// <summary>
    /// 主程序 Config 目录里的公钥、许可证和水位。路径由调用方传入。
    /// </summary>
    public sealed class LicenseDirectoryStore
    {
        /// <summary>
        /// 公钥文件名。内容是 SubjectPublicKeyInfo 的标准 Base64。
        /// </summary>
        public const string PublicKeyFileName = "license-public.spki";

        /// <summary>
        /// 许可证文件名。
        /// </summary>
        public const string LicenseFileName = "license.lic";

        /// <summary>
        /// 时钟水位文件名。8 字节小端 UTC ticks。
        /// </summary>
        public const string SeenFileName = "license-seen.dat";

        /// <summary>
        /// 这一组文件所在的目录。
        /// </summary>
        private readonly string _directory;

        /// <summary>
        /// 指定 Config 目录。目录可以尚未创建，写文件时再创建。
        /// </summary>
        public LicenseDirectoryStore(string configDirectory)
        {
            if (string.IsNullOrWhiteSpace(configDirectory))
                throw new ArgumentException("配置目录为空。", nameof(configDirectory));

            _directory = configDirectory;
        }

        /// <summary>
        /// 公钥路径。
        /// </summary>
        public string PublicKeyPath => Path.Combine(_directory, PublicKeyFileName);

        /// <summary>
        /// 许可证路径。
        /// </summary>
        public string LicensePath => Path.Combine(_directory, LicenseFileName);

        /// <summary>
        /// 水位路径。
        /// </summary>
        public string SeenPath => Path.Combine(_directory, SeenFileName);

        /// <summary>
        /// 读公钥。文件不存在是 <see cref="LicenseFailureCode.PublicKeyMissing"/>，内容坏了是 <see cref="LicenseFailureCode.PublicKeyInvalid"/>。
        /// </summary>
        public bool TryReadPublicKey(out byte[] subjectPublicKeyInfo, out LicenseFailureCode code)
        {
            subjectPublicKeyInfo = [];
            if (!File.Exists(PublicKeyPath))
            {
                code = LicenseFailureCode.PublicKeyMissing;
                return false;
            }

            try
            {
                var text = File.ReadAllText(PublicKeyPath);
                if (!TryDecodePublicKey(text, out subjectPublicKeyInfo))
                {
                    code = LicenseFailureCode.PublicKeyInvalid;
                    return false;
                }

                code = LicenseFailureCode.None;
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException)
            {
                code = LicenseFailureCode.PublicKeyInvalid;
                return false;
            }
        }

        /// <summary>
        /// 把公钥文件里的 Base64 收成 SubjectPublicKeyInfo。空内容和非法 Base64 都返回 false。
        /// </summary>
        public static bool TryDecodePublicKey(string text, out byte[] subjectPublicKeyInfo)
        {
            subjectPublicKeyInfo = [];
            if (string.IsNullOrWhiteSpace(text))
                return false;

            try
            {
                subjectPublicKeyInfo = Convert.FromBase64String(text.Trim());
                return subjectPublicKeyInfo.Length > 0;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// 读许可证字节。文件不存在时返回 false。读失败抛出原来的 IO 异常。
        /// </summary>
        public bool TryReadLicense(out byte[] utf8)
        {
            utf8 = [];
            if (!File.Exists(LicensePath))
                return false;

            utf8 = File.ReadAllBytes(LicensePath);
            return true;
        }

        /// <summary>
        /// 读水位。没有文件、长度不是 8，都当成没有水位。
        /// </summary>
        public DateTimeOffset? ReadSeen()
        {
            if (!File.Exists(SeenPath))
                return null;

            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(SeenPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return null;
            }

            if (bytes.Length != 8)
                return null;

            var ticks = System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(bytes);
            if (ticks < DateTimeOffset.MinValue.Ticks || ticks > DateTimeOffset.MaxValue.Ticks)
                return null;

            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

        /// <summary>
        /// 写入公钥的 Base64 文本。
        /// </summary>
        public void WritePublicKey(byte[] subjectPublicKeyInfo)
        {
            ArgumentNullException.ThrowIfNull(subjectPublicKeyInfo);
            if (subjectPublicKeyInfo.Length == 0)
                throw new ArgumentException("公钥为空。", nameof(subjectPublicKeyInfo));

            WriteText(PublicKeyPath, Convert.ToBase64String(subjectPublicKeyInfo) + "\n");
        }

        /// <summary>
        /// 写入许可证字节。目标已存在时替换，不留下半截文件。
        /// </summary>
        public void WriteLicense(byte[] utf8)
        {
            ArgumentNullException.ThrowIfNull(utf8);
            Directory.CreateDirectory(_directory);
            var temp = LicensePath + ".tmp";
            File.WriteAllBytes(temp, utf8);
            if (File.Exists(LicensePath))
                File.Replace(temp, LicensePath, null);
            else
                File.Move(temp, LicensePath);
        }

        /// <summary>
        /// 写入水位。调用方传入已经取过 max 的时刻。
        /// </summary>
        public void WriteSeen(DateTimeOffset instant)
        {
            var bytes = new byte[8];
            System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(bytes, instant.ToUniversalTime().UtcTicks);
            Directory.CreateDirectory(_directory);
            File.WriteAllBytes(SeenPath, bytes);
        }

        /// <summary>
        /// 删掉水位。签发程序把新证写到主程序目录时用，门窗导入不调用。
        /// </summary>
        public void DeleteSeen()
        {
            if (File.Exists(SeenPath))
                File.Delete(SeenPath);
        }

        /// <summary>
        /// 按 UTF-8 无 BOM 写文本。
        /// </summary>
        private void WriteText(string path, string text)
        {
            Directory.CreateDirectory(_directory);
            File.WriteAllText(path, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
    }
}
