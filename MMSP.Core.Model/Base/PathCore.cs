using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace MMSP.Core.Model.Base
{
    public class PathCore
    {
        /// <summary>
        /// 根盘符,默认C盘
        /// </summary>
        public static readonly string RootDisk = ConfigurationManager.AppSettings["RootDisk"] ?? "C";

        #region << 文件夹 >>

        /// <summary>
        /// 数据库文件夹名
        /// </summary>
        public const string Folder_DB = "DB";

        /// <summary>
        /// 日志文件夹名
        /// </summary>
        public const string Folder_Log = "Log";

        /// <summary>
        /// 配置文件夹名
        /// </summary>
        public const string Folder_Config = "Config";

        /// <summary>
        /// 客户数据根目录名。位于程序目录的上一层。
        /// </summary>
        public const string Folder_Data = "Data";

        /// <summary>
        /// 应用程序根目录
        /// </summary>
        public static readonly string Folder_App = AppDomain.CurrentDomain.BaseDirectory;

        #endregion

        #region << 文件名 >>

        /// <summary>
        /// sqlite数据库文件全名称，包含扩展名
        /// </summary>
        public static readonly string File_SqliteDB = "ldm.db";

        /// <summary>
        /// 配置文件全名称，包含扩展名
        /// </summary>
        public static readonly string File_Config = "Config.cfg";

        /// <summary>
        /// 系统配置库文件名。
        /// </summary>
        public const string File_SysConfig = "sys.cfg";

        /// <summary>
        /// NLog 配置文件名。源文件在 Lib\Config，编译后位于程序目录。
        /// </summary>
        public static readonly string File_Log = "NLog.config";

        #endregion

        #region << 全路径 >>

        /// <summary>
        /// 系统配置库全路径：程序目录\Config\sys.cfg。
        /// </summary>
        public static string SystemDbFilePath => Path.Combine(Folder_App, Folder_Config, File_SysConfig);

        /// <summary>
        /// 客户数据根目录：程序目录上一层\Data。
        /// </summary>
        public static string DataDirectory
        {
            get
            {
                var baseDir = Folder_App.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var parent = Directory.GetParent(baseDir);
                if (parent is null)
                    throw new InvalidOperationException("找不到程序目录的上一层，无法定位客户数据目录。");

                return Path.Combine(parent.FullName, Folder_Data);
            }
        }

        /// <summary>
        /// 客户数据库全路径：程序目录上一层\Data\DB\ldm.db。
        /// </summary>
        public static string CustomerDbFilePath => Path.Combine(DataDirectory, Folder_DB, File_SqliteDB);

        /// <summary>
        /// 日志目录：程序目录上一层\Data\Log。按年月日各写一个文件。
        /// </summary>
        public static string LogDirectory => Path.Combine(DataDirectory, Folder_Log);

        /// <summary>
        /// 客户数据库连接字符串。
        /// </summary>
        public static string ConnectionString_Sqlite => $"Data Source={CustomerDbFilePath}";

        /// <summary>
        /// 系统配置库连接字符串。
        /// </summary>
        public static string ConnectionString_SysConfig => $"Data Source={SystemDbFilePath}";

        /// <summary>
        /// 配置全路径
        /// </summary>
        public static readonly string ConfigPath = Path.Combine(Folder_App, Folder_Config, File_Config);

        /// <summary>
        /// 程序目录下的 NLog.config。日志内容写到 <see cref="LogDirectory"/>。
        /// </summary>
        public static readonly string LogFilePath = Path.Combine(Folder_App, File_Log);

        //RootDisk

        #endregion
    }
}
