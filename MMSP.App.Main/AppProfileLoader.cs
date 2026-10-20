using System.IO;
using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;

namespace MMSP.App.Main
{
    /// <summary>
    /// 从程序目录读取应用程序安装身份。
    /// </summary>
    internal static class AppProfileLoader
    {
        /// <summary>
        /// 读取 Config\app.cfg。文件不存在或内容无效时失败。
        /// </summary>
        public static AppProfile Load()
        {
            var path = PathCore.ConfigPath;
            if (!File.Exists(path))
                throw new InvalidOperationException($"找不到应用程序配置：{path}");

            return AppProfile.Parse(File.ReadAllText(path));
        }
    }
}
