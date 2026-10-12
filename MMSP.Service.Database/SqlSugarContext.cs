using MMSP.Core.Model.Base;
using MMSP.Core.Model.DB;
using SqlSugar;

namespace MMSP.Service.Database
{
    /// <summary>
    /// 两套 SQLite。系统配置库是程序目录下的 Config\sys.cfg，客户库是程序目录上一层的 Data\DB\ldm.db。
    /// </summary>
    public class SqlSugarContext : IDisposable
    {
        /// <summary>
        /// 防止重复打开。
        /// </summary>
        private static readonly object Gate = new();

        /// <summary>
        /// 系统配置库。未打开时为空。
        /// </summary>
        private static SqlSugarScope? _systemDb;

        /// <summary>
        /// 客户数据库。未打开时为空。
        /// </summary>
        private static SqlSugarScope? _customerDb;

        /// <summary>
        /// 是否已经打开。
        /// </summary>
        private static bool _opened;

        /// <summary>
        /// 系统配置库。菜单和用户表建在这里。
        /// </summary>
        public static SqlSugarScope SystemDb => _systemDb ?? throw new InvalidOperationException("系统配置库尚未打开。");

        /// <summary>
        /// 客户数据库。客户业务表随后注册到这个库。
        /// </summary>
        public static SqlSugarScope CustomerDb => _customerDb ?? throw new InvalidOperationException("客户数据库尚未打开。");

        /// <summary>
        /// 两个库是否已经打开。
        /// </summary>
        public static bool IsOpen => _opened;

        /// <summary>
        /// 创建目录并打开两个库。重复调用直接返回。
        /// </summary>
        public static void Open()
        {
            lock (Gate)
            {
                if (_opened)
                    return;

                _systemDb = CreateScope(PathCore.SystemDbFilePath, InitSystemTables);
                _customerDb = CreateScope(PathCore.CustomerDbFilePath, InitCustomerTables);
                _customerDb.Ado.GetScalar("PRAGMA journal_mode=WAL;");
                _opened = true;
            }
        }

        /// <summary>
        /// 建系统表。当前平台表都放在系统配置库。
        /// </summary>
        private static void InitSystemTables(SqlSugarScope db)
        {
            db.CodeFirst.InitTables(typeof(MenuEntity), typeof(UserEntity));
            MenuSeed.Ensure(db);
            UserSeed.Ensure(db);
        }

        /// <summary>
        /// 建客户表。运行日志放在客户库。
        /// </summary>
        private static void InitCustomerTables(SqlSugarScope db)
        {
            if (db.DbMaintenance.IsAnyTable("biz_logs", false))
                EnsureBizTypeColumn(db);

            db.CodeFirst.InitTables(typeof(RunLogEntity));
            db.Ado.ExecuteCommand("CREATE INDEX IF NOT EXISTS idx_biz_logs_log_time ON biz_logs (log_time);");
        }

        /// <summary>
        /// 给已经存在的运行日志表补业务类型列。没有这列时，旧行记为 Sys。
        /// </summary>
        private static void EnsureBizTypeColumn(SqlSugarScope db)
        {
            var count = db.Ado.GetInt("SELECT COUNT(1) FROM pragma_table_info('biz_logs') WHERE name = 'biz_type'");
            if (count > 0)
                return;

            db.Ado.ExecuteCommand("ALTER TABLE biz_logs ADD COLUMN biz_type INTEGER NOT NULL DEFAULT 0");
        }

        /// <summary>
        /// 确保目录存在，然后创建 SQLite 文件。
        /// </summary>
        private static SqlSugarScope CreateScope(string filePath, Action<SqlSugarScope> initTables)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var scope = new SqlSugarScope(new ConnectionConfig
            {
                DbType = DbType.Sqlite,
                ConnectionString = $"Data Source={filePath}",
                LanguageType = LanguageType.Default,
                IsAutoCloseConnection = true
            },
            db =>
            {
                db.Aop.OnLogExecuting = (sql, _) => Console.WriteLine(sql);
                db.Ado.CommandTimeOut = 10;
            });

            scope.DbMaintenance.CreateDatabase();
            initTables(scope);
            return scope;
        }

        /// <summary>
        /// 关闭两个库。
        /// </summary>
        public void Dispose()
        {
            lock (Gate)
            {
                _systemDb?.Dispose();
                _customerDb?.Dispose();
                _systemDb = null;
                _customerDb = null;
                _opened = false;
            }
        }
    }
}
