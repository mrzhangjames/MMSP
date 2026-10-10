using System.Reflection;

namespace MMSP.Common.Helper
{
    /// <summary>
    /// 读取正在运行的主程序版本号。
    /// </summary>
    public static class AppVersionHelper
    {
        /// <summary>
        /// 返回主程序版本，格式为 V{主}.{次}.{生成}.{修订}。
        /// 前三位来自入口程序集里手写的版本，最后一位是编译器生成的修订号。读不到版本时返回 V1.0.0.0。
        /// </summary>
        public static string GetVersionText()
        {
            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            if (version is null)
                return "V1.0.0.0";

            var build = version.Build < 0 ? 0 : version.Build;
            var revision = version.Revision < 0 ? 0 : version.Revision;
            return $"V{version.Major}.{version.Minor}.{build}.{revision}";
        }
    }
}
