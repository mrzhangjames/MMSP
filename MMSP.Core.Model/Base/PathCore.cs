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
        /// 日志文件全名称，包含扩展名
        /// </summary>
        public static readonly string File_Log = "NLog.config";

        #endregion

        #region << 全路径 >>

        /// <summary>
        /// sqlite数据库连接字符串
        /// </summary>
        public static readonly string ConnectionString_Sqlite = $"Data Source={Path.Combine(Folder_App, Folder_DB, File_SqliteDB)}";

        /// <summary>
        /// 配置全路径
        /// </summary>
        public static readonly string ConfigPath = Path.Combine(Folder_App, Folder_Config, File_Config);

        /// <summary>
        /// 日志全路径
        /// </summary>
        public static readonly string LogFilePath = Path.Combine(Folder_App, File_Log);

        //RootDisk

        #endregion
    }
}
