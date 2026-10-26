using MMSP.Plugin.Lux.WMI;

namespace MMSP.Plugin.Lux.Tests
{
    /// <summary>
    /// WMI 的加密、报文和目录。不访问工厂地址。
    /// </summary>
    public class LuxWmiTests
    {
        /// <summary>
        /// 测试用的 32 字节密钥。不是现场密钥。
        /// </summary>
        private const string Key = "12345678901234567890123456789012";

        /// <summary>
        /// 32 字节密钥可以往返。短密钥直接失败。
        /// </summary>
        [Fact]
        public void Crypto_RoundTrip_Requires32ByteKey()
        {
            Assert.True(LuxWmiCrypto.TryEncrypt("过站", Key, out var cipher, out var error), error);
            Assert.True(LuxWmiCrypto.TryDecrypt(cipher, Key, out var plain, out error), error);
            Assert.Equal("过站", plain);
            Assert.False(LuxWmiCrypto.TryEncrypt("过站", "short", out _, out error));
            Assert.Equal("AES 密钥长度不正确", error);
        }

        /// <summary>
        /// 明文带协议字段。SOAP 里只有密文，不再出现密码。
        /// </summary>
        [Fact]
        public void Envelope_SoapHidesPlainPassword()
        {
            var plain = LuxWmiEnvelope.Plain(
                "OPPO",
                "MES_BA_WI6011",
                new Dictionary<string, object?> { ["BarCode"] = "SN1" },
                parameterAsString: false);
            Assert.Contains("\"method\":\"MES_BA_WI6011\"", plain, StringComparison.Ordinal);
            Assert.Contains("\"BarCode\":\"SN1\"", plain, StringComparison.Ordinal);
            Assert.True(LuxWmiCrypto.TryEncrypt(plain, Key, out var cipher, out var error), error);
            var soap = LuxWmiEnvelope.Soap(cipher);
            Assert.Contains("<WMI01 xmlns=\"http://tempuri.org/\">", soap, StringComparison.Ordinal);
            Assert.Contains("<Par>", soap, StringComparison.Ordinal);
            Assert.DoesNotContain("sysuser", soap, StringComparison.Ordinal);
        }

        /// <summary>
        /// 老示例把 methodparameter 写成字符串。
        /// </summary>
        [Fact]
        public void Envelope_StringParameter_IsJsonText()
        {
            var plain = LuxWmiEnvelope.Plain(
                "OPPO",
                "MES_BA_WI6018",
                new Dictionary<string, object?> { ["testWorkID"] = "SH05090" },
                parameterAsString: true);
            Assert.Contains("\"methodparameter\":\"{", plain, StringComparison.Ordinal);
            Assert.Contains("testWorkID", plain, StringComparison.Ordinal);
        }

        /// <summary>
        /// 服务地址优先于 URL 路径。
        /// </summary>
        [Fact]
        public void Address_PrefersServiceUrl()
        {
            var config = new LuxMesWmiConfig
            {
                ServiceUrl = " http://line/WMI.asmx ",
                UrlPath = "http://other/WMI.asmx"
            };
            Assert.Equal("http://line/WMI.asmx", LuxWmiEnvelope.AddressOf(config));
            config.ServiceUrl = "  ";
            Assert.Equal("http://other/WMI.asmx", LuxWmiEnvelope.AddressOf(config));
        }

        /// <summary>
        /// ASMX 的 string 节点里，state 为 true 才成功。
        /// </summary>
        [Fact]
        public void Response_ReadsStateContentAndError()
        {
            const string ok = """
                <?xml version="1.0" encoding="utf-8"?>
                <string xmlns="http://tempuri.org/">{
                  "state": "true",
                  "content": "true"
                }</string>
                """;
            Assert.True(LuxWmiEnvelope.TryRead(ok, out var state, out var content, out _));
            Assert.True(LuxWmiEnvelope.IsOk(state));
            Assert.Equal("true", content);

            const string bad = """
                <string xmlns="http://tempuri.org/">{
                  "state": "false",
                  "error": "sys",
                  "content": "工单不存在"
                }</string>
                """;
            Assert.True(LuxWmiEnvelope.TryRead(bad, out state, out content, out var error));
            Assert.False(LuxWmiEnvelope.IsOk(state));
            Assert.Equal("sys", error);
            Assert.Equal("工单不存在", content);
        }

        /// <summary>
        /// 未启用、空地址和密钥长度不对时不发请求。
        /// </summary>
        [Fact]
        public void Call_RejectsDisabledEmptyAddressAndBadKey()
        {
            var disabled = new LuxMesWmiConfig { IsEnable = false, ServiceUrl = "http://127.0.0.1:9/WMI.asmx", AesKey = Key };
            Assert.Equal("WMI 未启用", LuxWmiClient.Call(disabled, LuxWmiCatalog.MES_BA_WI6011).Error);

            var empty = new LuxMesWmiConfig { IsEnable = true, AesKey = Key };
            Assert.Equal("服务地址为空", LuxWmiClient.Call(empty, LuxWmiCatalog.MES_BA_WI6011).Error);

            var badKey = new LuxMesWmiConfig { IsEnable = true, ServiceUrl = "http://127.0.0.1:9/WMI.asmx", AesKey = "short" };
            var failed = LuxWmiClient.Call(badKey, LuxWmiCatalog.MES_BA_WI6011);
            Assert.Equal("AES 密钥长度不正确", failed.Error);
            Assert.DoesNotContain(Key, failed.Error, StringComparison.Ordinal);
        }

        /// <summary>
        /// 两份目录合并后方法不重复，并带上 5.0.5 的权限字段和仅 5.0.3 有的方法。
        /// </summary>
        [Fact]
        public void Catalog_MergesBothDocuments()
        {
            var names = LuxWmiCatalog.All.Select(item => item.Name).ToList();
            Assert.Equal(249, names.Count);
            Assert.Equal(names.Count, names.Distinct(StringComparer.Ordinal).Count());
            Assert.DoesNotContain("MES_BA_WI601", names);
            Assert.Equal(["Account", "FormName", "Auth", "Line", "StationName"], LuxWmiCatalog.MES_BA_WI6014.Fields);
            Assert.Equal("已取消", LuxWmiCatalog.MES_BA_WI6012.Status);
            Assert.True(LuxWmiCatalog.MES_BA_WI6018.ParameterAsString);
            Assert.Equal("仅 5.0.3", LuxWmiCatalog.QUERY_RECORD.Status);
            Assert.Equal("仅 5.0.3", LuxWmiCatalog.MES_PA_WI1011.Status);
            Assert.Equal("仅 5.0.3", LuxWmiCatalog.MES_AS_WI1032.Status);
            Assert.Contains(LuxWmiCatalog.All, item => item.Name == "MES_SAP_WIA008");
        }
    }
}
