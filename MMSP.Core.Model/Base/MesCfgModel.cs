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
        public string UrlPath { get; set; } = "";
    }
}
