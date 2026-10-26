using System.ComponentModel;
using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;

namespace MMSP.Plugin.Lux.WMI
{
    /// <summary>
    /// Lux WMI 的 MES 配置。
    /// </summary>
    [Customer(Customer.Lux)]
    [DeviceType(DeviceType.PAD_001)]
    public class LuxMesWmiConfig : MesCfgModel
    {
        /// <summary>
        /// 完整服务地址。有值时优先于 URL 路径。不写死工厂地址。
        /// </summary>
        [Description("服务地址")]
        public string ServiceUrl { get; set; } = "";

        /// <summary>
        /// 报文里的 servername。由现场填写，例如客户名。
        /// </summary>
        [Description("客户名")]
        public string ServerName { get; set; } = "";

        /// <summary>
        /// AES-256 密钥。必须是 32 字节的 UTF-8 文本。只保存在 MES 配置里。
        /// </summary>
        [Description("AES 密钥")]
        public string AesKey { get; set; } = "";

        /// <summary>
        /// 单次调用的超时，毫秒。小于等于 0 时按 10 秒。
        /// </summary>
        [Description("超时")]
        public int TimeoutMs { get; set; } = 10000;

        /// <summary>
        /// 第一次失败之后再试的次数。
        /// </summary>
        [Description("重试次数")]
        public int RetryCount { get; set; }
    }
}

