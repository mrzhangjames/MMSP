using System.ComponentModel;

namespace MMSP.Core.Model.Base
{
    /// <summary>
    /// MES 配置基类。具体 MES 配置继承它。
    /// </summary>
    public class MesCfgModel
    {
        /// <summary>
        /// URL 路径。
        /// </summary>
        [Description("URL 路径")]
        public string UrlPath { get; set; } = "";

        /// <summary>
        /// 是否启用。
        /// </summary>
        [Description("是否启用")]
        public bool IsEnable {  get; set; } = true;
    }
}
