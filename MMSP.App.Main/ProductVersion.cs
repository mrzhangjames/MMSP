using System.Reflection;

[assembly: AssemblyVersion(MMSP.App.Main.ProductVersion.Value)]

namespace MMSP.App.Main
{
    /// <summary>
    /// 主程序版本。前三位由开发人员手动修改，最后一位保持星号。
    /// 编译器把星号写成当天 0 点起的秒数除以 2。
    /// </summary>
    public static class ProductVersion
    {
        /// <summary>
        /// 程序集版本。开发人员只改星号前面的三段。
        /// </summary>
        public const string Value = "1.0.0.*";
    }
}
