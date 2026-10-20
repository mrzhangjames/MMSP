using System.Security.Cryptography;
using System.Text;
using MMSP.Core.License;

namespace MMSP.Core.License.Tests
{
    /// <summary>
    /// 规范字节、签发和校验顺序。
    /// </summary>
    public class LicenseValidatorTests
    {
        /// <summary>
        /// 设计文档锁死的机器证样例。
        /// </summary>
        private static readonly Guid SampleId = Guid.Parse("3f1c0c2e-7a1b-4d5e-9c11-0a0b0c0d0e0f");

        /// <summary>
        /// 样例里的生效和签发时刻。
        /// </summary>
        private static readonly DateTimeOffset SampleStart = DateTimeOffset.Parse("2026-09-28T00:00:00.0000000+00:00");

        /// <summary>
        /// 样例里的到期时刻。
        /// </summary>
        private static readonly DateTimeOffset SampleEnd = DateTimeOffset.Parse("2027-09-28T00:00:00.0000000+00:00");

        /// <summary>
        /// 样例第 4 行。64 位小写十六进制。
        /// </summary>
        private const string SampleHex = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

        /// <summary>
        /// 有模块的 8 行哈希和长度。
        /// </summary>
        [Fact]
        public void Canonical_SampleWithModule_MatchesLockedHash()
        {
            var bytes = SampleCanonical([LicenseModules.TemplateMatch]);
            Assert.Equal(234, bytes.Length);
            Assert.Equal(0x0A, bytes[^1]);
            Assert.Equal("a5cb03cd4d209349dc65b137b5e8d753feef25841dfa95510d1b1c09c915f581", Hash(bytes));
        }

        /// <summary>
        /// 空模块时末尾是两个 LF。
        /// </summary>
        [Fact]
        public void Canonical_EmptyModules_MatchesLockedHash()
        {
            var bytes = SampleCanonical([]);
            Assert.Equal(212, bytes.Length);
            Assert.Equal("d828a62e47177f2572ecf800c9db6c59c4e3d3349b526f8d511c43769ccddebe", Hash(bytes));
        }

        /// <summary>
        /// 同一 Guid 的两种写法得到同一个指纹。空串和非法 Guid 读不到。
        /// </summary>
        [Fact]
        public void Fingerprint_NormalizesGuid_AndRejectsBlank()
        {
            var guid = Guid.Parse("3f1c0c2e-7a1b-4d5e-9c11-0a0b0c0d0e0f");
            Assert.True(MachineFingerprintFormat.TryFromMachineGuid(guid.ToString("D").ToUpperInvariant(), out var lower));
            Assert.True(MachineFingerprintFormat.TryFromMachineGuid("{" + guid.ToString("D").ToUpperInvariant() + "}", out var braced));
            Assert.Equal(lower, braced);
            Assert.True(MachineFingerprintFormat.IsMachineHex(lower));
            Assert.False(MachineFingerprintFormat.TryFromMachineGuid("  ", out _));
            Assert.False(MachineFingerprintFormat.TryFromMachineGuid("not-a-guid", out _));
        }

        /// <summary>
        /// 通用证在有效期内通过，并且不比较本机指纹。
        /// </summary>
        [Fact]
        public void Sign_Universal_ValidInsideWindow()
        {
            var (validator, privateKey) = Pair();
            var now = SampleStart.AddDays(1);
            var bytes = Sign(privateKey, MachineFingerprintFormat.Universal, SampleStart, SampleEnd, [LicenseModules.TemplateMatch, LicenseModules.Lux]);
            var result = validator.Evaluate(bytes, "", now, null, TimeSpan.Zero);
            Assert.Equal(LicenseFailureCode.None, result.Code);
            Assert.Equal("DEMO", result.CustomerCode);
            Assert.True(result.IsUniversal);
            Assert.Equal([LicenseModules.TemplateMatch, LicenseModules.Lux], result.Modules);
        }

        /// <summary>
        /// 改客户编码后签名失败，而且不带出证书内容。
        /// </summary>
        [Fact]
        public void Sign_ChangedCustomer_IsSignatureInvalid()
        {
            var (validator, privateKey) = Pair();
            var text = Encoding.UTF8.GetString(Sign(privateKey, MachineFingerprintFormat.Universal, SampleStart, SampleEnd, [LicenseModules.TemplateMatch]));
            text = text.Replace("DEMO", "DEMOX", StringComparison.Ordinal);
            var result = validator.Evaluate(Encoding.UTF8.GetBytes(text), "", SampleStart, null, TimeSpan.Zero);
            Assert.Equal(LicenseFailureCode.SignatureInvalid, result.Code);
            Assert.Null(result.CustomerCode);
        }

        /// <summary>
        /// 模块顺序参与签名，不能自行排序。
        /// </summary>
        [Fact]
        public void Sign_ModuleOrder_IsSignificant()
        {
            var (validator, privateKey) = Pair();
            var bytes = Sign(privateKey, MachineFingerprintFormat.Universal, SampleStart, SampleEnd, [LicenseModules.Lux, LicenseModules.TemplateMatch]);
            var result = validator.Evaluate(bytes, "", SampleStart, null, TimeSpan.Zero);
            Assert.Equal([LicenseModules.Lux, LicenseModules.TemplateMatch], result.Modules);

            var text = Encoding.UTF8.GetString(bytes);
            var swapped = text
                .Replace("\"MMSP.Plugin.Lux\"", "\"__LUX__\"", StringComparison.Ordinal)
                .Replace("\"MMSP.App.TemplateMatch\"", "\"MMSP.Plugin.Lux\"", StringComparison.Ordinal)
                .Replace("\"__LUX__\"", "\"MMSP.App.TemplateMatch\"", StringComparison.Ordinal);
            Assert.NotEqual(text, swapped);
            var broken = validator.Evaluate(Encoding.UTF8.GetBytes(swapped), "", SampleStart, null, TimeSpan.Zero);
            Assert.Equal(LicenseFailureCode.SignatureInvalid, broken.Code);
        }

        /// <summary>
        /// 把机器证改成 universal 不能绕过签名。
        /// </summary>
        [Fact]
        public void Sign_RewritingMachineAsUniversal_IsSignatureInvalid()
        {
            var (validator, privateKey) = Pair();
            var fingerprint = MachineFingerprintFormat.FromGuid(SampleId);
            var bytes = Sign(privateKey, fingerprint, SampleStart, SampleEnd, [LicenseModules.TemplateMatch]);
            var text = Encoding.UTF8.GetString(bytes).Replace(fingerprint, MachineFingerprintFormat.Universal, StringComparison.Ordinal);
            var result = validator.Evaluate(Encoding.UTF8.GetBytes(text), "", SampleStart, null, TimeSpan.Zero);
            Assert.Equal(LicenseFailureCode.SignatureInvalid, result.Code);
        }

        /// <summary>
        /// 机器证只接受同一份指纹。读不到指纹和对不上是两个码。
        /// </summary>
        [Fact]
        public void Sign_MachineLicense_ChecksFingerprint()
        {
            var (validator, privateKey) = Pair();
            var fingerprint = MachineFingerprintFormat.FromGuid(SampleId);
            var bytes = Sign(privateKey, fingerprint, SampleStart, SampleEnd, []);
            Assert.Equal(LicenseFailureCode.None, validator.Evaluate(bytes, fingerprint, SampleStart, null, TimeSpan.Zero).Code);
            Assert.Equal(LicenseFailureCode.MachineUnreadable, validator.Evaluate(bytes, "", SampleStart, null, TimeSpan.Zero).Code);
            var other = MachineFingerprintFormat.FromGuid(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"));
            Assert.Equal(LicenseFailureCode.MachineMismatch, validator.Evaluate(bytes, other, SampleStart, null, TimeSpan.Zero).Code);
        }

        /// <summary>
        /// 指纹写法不对时，在验签之前就是格式错误。
        /// </summary>
        [Fact]
        public void Format_RejectsUniversalSynonymsAndShortHex()
        {
            var (validator, privateKey) = Pair();
            var bytes = Sign(privateKey, MachineFingerprintFormat.Universal, SampleStart, SampleEnd, []);
            var text = Encoding.UTF8.GetString(bytes);
            Assert.Equal(LicenseFailureCode.FormatInvalid, EvaluateText(validator, text.Replace("universal", "Universal", StringComparison.Ordinal)));
            Assert.Equal(LicenseFailureCode.FormatInvalid, EvaluateText(validator, text.Replace("universal", SampleHex[..63], StringComparison.Ordinal)));
            Assert.Equal(LicenseFailureCode.FormatInvalid, EvaluateText(validator, text.Replace("universal", SampleHex.ToUpperInvariant(), StringComparison.Ordinal)));
        }

        /// <summary>
        /// 尾部多余字节、未知属性和重复模块都是格式错误。
        /// </summary>
        [Fact]
        public void Format_RejectsTrailingByteUnknownPropertyAndDuplicateModule()
        {
            var (validator, privateKey) = Pair();
            var bytes = Sign(privateKey, MachineFingerprintFormat.Universal, SampleStart, SampleEnd, [LicenseModules.TemplateMatch]);
            var trailed = bytes.Concat(new byte[] { 0x78 }).ToArray();
            Assert.Equal(LicenseFailureCode.FormatInvalid, validator.Evaluate(trailed, "", SampleStart, null, TimeSpan.Zero).Code);

            var text = Encoding.UTF8.GetString(bytes);
            var extra = text.Replace("\"format\": 1", "\"format\": 1,\r\n  \"extra\": 1", StringComparison.Ordinal);
            Assert.Equal(LicenseFailureCode.FormatInvalid, EvaluateText(validator, extra));

            var duplicated = text.Replace(
                "\"MMSP.App.TemplateMatch\"",
                "\"MMSP.App.TemplateMatch\", \"MMSP.App.TemplateMatch\"",
                StringComparison.Ordinal);
            Assert.Equal(LicenseFailureCode.FormatInvalid, EvaluateText(validator, duplicated));
        }

        /// <summary>
        /// 空字节是格式错误。负容差在签名和机器比较之后返回。
        /// </summary>
        [Fact]
        public void Evaluate_EmptyBytesAndNegativeTolerance()
        {
            var (validator, privateKey) = Pair();
            Assert.Equal(LicenseFailureCode.FormatInvalid, validator.Evaluate([], "", SampleStart, null, TimeSpan.Zero).Code);
            var bytes = Sign(privateKey, MachineFingerprintFormat.Universal, SampleStart, SampleEnd, []);
            var result = validator.Evaluate(bytes, "", SampleStart, null, TimeSpan.FromMinutes(-1));
            Assert.Equal(LicenseFailureCode.ToleranceInvalid, result.Code);
            Assert.Equal("DEMO", result.CustomerCode);
        }

        /// <summary>
        /// 早于水位减容差是回拨。容差之内仍然有效。
        /// </summary>
        [Fact]
        public void Evaluate_ClockRollback_UsesTolerance()
        {
            var (validator, privateKey) = Pair();
            var bytes = Sign(privateKey, MachineFingerprintFormat.Universal, SampleStart, SampleEnd, []);
            var seen = SampleStart.AddHours(2);
            Assert.Equal(
                LicenseFailureCode.ClockRollback,
                validator.Evaluate(bytes, "", seen.AddMinutes(-6), seen, TimeSpan.FromMinutes(5)).Code);
            Assert.Equal(
                LicenseFailureCode.None,
                validator.Evaluate(bytes, "", seen.AddMinutes(-5), seen, TimeSpan.FromMinutes(5)).Code);
        }

        /// <summary>
        /// 底栏四种状态：未授权、已授权、七天内到期、授权到期。
        /// </summary>
        [Fact]
        public void Footer_SwitchesAtSevenDaysAndExpiry()
        {
            var end = SampleStart.AddDays(30);
            var valid = new LicenseEvaluation
            {
                Code = LicenseFailureCode.None,
                NotAfterUtc = end
            };
            Assert.Equal(LicenseFooterState.Authorized, LicenseFooter.StateOf(valid, end.AddDays(-8)));
            Assert.Equal(LicenseFooterState.Authorized, LicenseFooter.StateOf(valid, end - LicenseFooter.Reminder - TimeSpan.FromTicks(1)));
            Assert.Equal(LicenseFooterState.Expiring, LicenseFooter.StateOf(valid, end - LicenseFooter.Reminder));
            Assert.Equal(LicenseFooterState.Expiring, LicenseFooter.StateOf(valid, end));

            var perpetual = new LicenseEvaluation
            {
                Code = LicenseFailureCode.None,
                NotAfterUtc = LicenseDates.PerpetualNotAfter
            };
            Assert.Equal(LicenseFooterState.Authorized, LicenseFooter.StateOf(perpetual, SampleStart));

            var expired = new LicenseEvaluation
            {
                Code = LicenseFailureCode.Expired,
                NotAfterUtc = end
            };
            Assert.Equal(LicenseFooterState.Expired, LicenseFooter.StateOf(expired, end.AddTicks(1)));
            Assert.Equal(LicenseFooterState.Unauthorized, LicenseFooter.StateOf(LicenseEvaluation.Only(LicenseFailureCode.FileMissing), SampleStart));
            Assert.Equal(
                LicenseFooterState.Unauthorized,
                LicenseFooter.StateOf(new LicenseEvaluation { Code = LicenseFailureCode.NotYetValid, NotAfterUtc = end }, SampleStart));
        }

        /// <summary>
        /// 导入时用导出的公钥验许可证，不用磁盘上的另一把公钥。
        /// </summary>
        [Fact]
        public void CheckPair_AcceptsMatchingKey_AndRejectsAnother()
        {
            var (_, privateKey) = Pair();
            var bytes = Sign(privateKey, MachineFingerprintFormat.Universal, SampleStart, SampleEnd, [LicenseModules.TemplateMatch]);
            var publicKey = LicenseKeys.PublicFromPrivate(privateKey);
            Assert.Equal(
                LicenseFailureCode.None,
                LicenseRuntime.CheckPair(publicKey, bytes, "", null, SampleStart).Code);

            var (otherPrivate, _) = LicenseKeys.Generate();
            var otherPublic = LicenseKeys.PublicFromPrivate(otherPrivate);
            Assert.Equal(
                LicenseFailureCode.SignatureInvalid,
                LicenseRuntime.CheckPair(otherPublic, bytes, "", null, SampleStart).Code);
        }

        /// <summary>
        /// 带本地时区的起止时间写进 JSON 后仍能验过。永久到期时刻同样可以。
        /// </summary>
        [Fact]
        public void Sign_LocalOffsetAndPerpetual_RoundTrip()
        {
            var (validator, privateKey) = Pair();
            var start = new DateTimeOffset(2026, 10, 20, 0, 0, 0, TimeSpan.FromHours(8));
            var end = new DateTimeOffset(2027, 10, 20, 23, 59, 59, TimeSpan.FromHours(8));
            var bytes = Sign(privateKey, MachineFingerprintFormat.Universal, start, end, []);
            Assert.Equal(LicenseFailureCode.None, validator.Evaluate(bytes, "", start.AddDays(1), null, TimeSpan.Zero).Code);

            var perpetual = Sign(privateKey, MachineFingerprintFormat.Universal, start, LicenseDates.PerpetualNotAfter, []);
            var result = validator.Evaluate(perpetual, "", start.AddDays(1), null, TimeSpan.Zero);
            Assert.Equal(LicenseFailureCode.None, result.Code);
            Assert.True(LicenseDates.IsPerpetual(result.NotAfterUtc!.Value));
        }

        /// <summary>
        /// 起止时刻都算有效。早一秒尚未生效，晚一秒已经过期。
        /// </summary>
        [Fact]
        public void Evaluate_TimeWindow_IsInclusive()
        {
            var (validator, privateKey) = Pair();
            var bytes = Sign(privateKey, MachineFingerprintFormat.Universal, SampleStart, SampleEnd, []);
            Assert.Equal(LicenseFailureCode.None, validator.Evaluate(bytes, "", SampleStart, null, TimeSpan.Zero).Code);
            Assert.Equal(LicenseFailureCode.None, validator.Evaluate(bytes, "", SampleEnd, null, TimeSpan.Zero).Code);
            Assert.Equal(LicenseFailureCode.NotYetValid, validator.Evaluate(bytes, "", SampleStart.AddTicks(-1), null, TimeSpan.Zero).Code);
            Assert.Equal(LicenseFailureCode.Expired, validator.Evaluate(bytes, "", SampleEnd.AddTicks(1), null, TimeSpan.Zero).Code);
        }

        /// <summary>
        /// 拼设计文档里的 8 行。
        /// </summary>
        private static byte[] SampleCanonical(IReadOnlyList<string> modules)
        {
            return LicenseCanonical.Build(1, SampleId, "DEMO", SampleHex, SampleStart, SampleEnd, SampleStart, modules);
        }

        /// <summary>
        /// 小写 SHA-256。
        /// </summary>
        private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

        /// <summary>
        /// 生成一对只活在这次测试里的密钥。
        /// </summary>
        private static (LicenseValidator Validator, byte[] PrivateKey) Pair()
        {
            var (privateKey, publicKey) = LicenseKeys.Generate();
            return (new LicenseValidator(publicKey), privateKey);
        }

        /// <summary>
        /// 用测试私钥签一份样例证。
        /// </summary>
        private static byte[] Sign(byte[] privateKey, string fingerprint, DateTimeOffset start, DateTimeOffset end, IReadOnlyList<string> modules)
        {
            return LicenseIssuer.Sign(new LicenseDraft
            {
                LicenseId = SampleId,
                CustomerCode = "DEMO",
                MachineFingerprint = fingerprint,
                NotBeforeUtc = start,
                NotAfterUtc = end,
                IssuedAtUtc = start,
                Modules = modules
            }, privateKey);
        }

        /// <summary>
        /// 用改过的 JSON 文本做一次校验。
        /// </summary>
        private static LicenseFailureCode EvaluateText(LicenseValidator validator, string text)
        {
            return validator.Evaluate(Encoding.UTF8.GetBytes(text), "", SampleStart, null, TimeSpan.Zero).Code;
        }
    }
}
