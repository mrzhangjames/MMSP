using MMSP.Core.Model.Base;
using MMSP.Core.Model.DB;
using MMSP.Service.Database.Seed;
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
        /// 客户数据库。运行日志、图表、UPH、PLC 连接、PLC 点位、扫码枪、流程和 MES 配置建在这里。
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
            db.CodeFirst.InitTables(typeof(MenuEntity), typeof(UserEntity), typeof(AppConfigEntity));
            MenuSeed.Ensure(db);
            UserSeed.Ensure(db);
            AppConfigSeed.Ensure(db);
        }

        /// <summary>
        /// 建客户表。运行日志、图表、UPH、PLC 连接、PLC 点位、扫码枪、流程和 MES 配置放在客户库。
        /// </summary>
        private static void InitCustomerTables(SqlSugarScope db)
        {
            if (db.DbMaintenance.IsAnyTable("biz_logs", false))
                EnsureBizTypeColumn(db);
            if (db.DbMaintenance.IsAnyTable("biz_flow_mes", false))
                DropLegacyFlowMes(db);
            TrimLegacyFlowSteps(db);
            EnsurePlcPointColumns(db);

            db.CodeFirst.InitTables(
                typeof(RunLogEntity),
                typeof(ChartEntity),
                typeof(UphEntity),
                typeof(PlcLinkEntity),
                typeof(PlcPointEntity),
                typeof(ScannerEntity),
                typeof(FlowEntity),
                typeof(FlowStepEntity),
                typeof(FlowVarEntity),
                typeof(FlowMesEntity));
            db.Ado.ExecuteCommand("CREATE INDEX IF NOT EXISTS idx_biz_logs_log_time ON biz_logs (log_time);");
            db.Ado.ExecuteCommand("CREATE UNIQUE INDEX IF NOT EXISTS idx_biz_uph_hour_time ON biz_uph (hour_time);");
            db.Ado.ExecuteCommand("CREATE UNIQUE INDEX IF NOT EXISTS idx_biz_flow_mes_config_type ON biz_flow_mes (config_type);");
            db.Ado.ExecuteCommand("CREATE UNIQUE INDEX IF NOT EXISTS idx_biz_flow_vars_flow_name ON biz_flow_vars (flow_id, name);");
            if (db.DbMaintenance.IsAnyTable("biz_flow_steps", false))
                EnsureCfgValueColumn(db);
        }

        /// <summary>
        /// 旧表按接口地址等五列保存，对不上配置类。没有 config_type 时丢掉重建，不迁移旧值。
        /// </summary>
        private static void DropLegacyFlowMes(SqlSugarScope db)
        {
            var count = db.Ado.GetInt("SELECT COUNT(1) FROM pragma_table_info('biz_flow_mes') WHERE name = 'config_type'");
            if (count > 0)
                return;

            db.Ado.ExecuteCommand("DROP TABLE biz_flow_mes");
        }

        /// <summary>
        /// 旧流程步上已被工具配置取代的列。
        /// </summary>
        private static readonly string[] LegacyFlowStepColumns =
        [
            "target",
            "data_type",
            "data_length",
            "write_value",
            "number_value",
            "text_value"
        ];

        /// <summary>
        /// 删掉流程步表里不再使用的列，并保留主键、流程、顺序、工具、标题和配置。
        /// CodeFirst 不会删除已有列，所以有旧列时先改名，再建新表，再拷回。
        /// </summary>
        internal static void TrimLegacyFlowSteps(SqlSugarScope db)
        {
            if (db.DbMaintenance.IsAnyTable("biz_flow_steps_legacy", false))
            {
                if (!db.DbMaintenance.IsAnyTable("biz_flow_steps", false))
                    db.CodeFirst.InitTables(typeof(FlowStepEntity));
                CopyLegacyFlowSteps(db);
                return;
            }

            if (!db.DbMaintenance.IsAnyTable("biz_flow_steps", false) || !HasLegacyFlowStepColumn(db))
                return;

            EnsureCfgValueColumn(db);
            db.Ado.ExecuteCommand("ALTER TABLE biz_flow_steps RENAME TO biz_flow_steps_legacy");
            db.CodeFirst.InitTables(typeof(FlowStepEntity));
            CopyLegacyFlowSteps(db);
        }

        /// <summary>
        /// 旧表是否还带着已经不用的列。
        /// </summary>
        private static bool HasLegacyFlowStepColumn(SqlSugarScope db)
        {
            var names = string.Join(", ", LegacyFlowStepColumns.Select(name => "'" + name + "'"));
            var count = db.Ado.GetInt(
                "SELECT COUNT(1) FROM pragma_table_info('biz_flow_steps') WHERE name IN (" + names + ")");
            return count > 0;
        }

        /// <summary>
        /// 把旧表里仍使用的列拷进新表，然后丢掉旧表。新表已有数据时不再拷，避免重复主键。
        /// </summary>
        private static void CopyLegacyFlowSteps(SqlSugarScope db)
        {
            db.Ado.BeginTran();
            try
            {
                var copied = db.Ado.GetInt("SELECT COUNT(1) FROM biz_flow_steps");
                if (copied == 0)
                {
                    db.Ado.ExecuteCommand(
                        """
                        INSERT INTO biz_flow_steps ("Id", flow_id, sort_index, tool_kind, title, cfg_value)
                        SELECT "Id", flow_id, sort_index, tool_kind, title, IFNULL(cfg_value, '')
                        FROM biz_flow_steps_legacy
                        """);
                }

                db.Ado.ExecuteCommand("DROP TABLE biz_flow_steps_legacy");
                db.Ado.CommitTran();
            }
            catch
            {
                db.Ado.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// 给已经存在的流程步表补工具配置列。
        /// </summary>
        private static void EnsureCfgValueColumn(SqlSugarScope db)
        {
            var count = db.Ado.GetInt("SELECT COUNT(1) FROM pragma_table_info('biz_flow_steps') WHERE name = 'cfg_value'");
            if (count > 0)
                return;

            db.Ado.ExecuteCommand("ALTER TABLE biz_flow_steps ADD COLUMN cfg_value TEXT NOT NULL DEFAULT ''");
        }

        /// <summary>
        /// 给已经存在的点位表补类型、Bit 位和描述。旧行类型为流程，Bit 位为空，描述为空。
        /// </summary>
        internal static void EnsurePlcPointColumns(SqlSugarScope db)
        {
            if (!db.DbMaintenance.IsAnyTable("biz_plc_points", false))
                return;

            AddPlcPointColumn(db, "point_kind", "ALTER TABLE biz_plc_points ADD COLUMN point_kind INTEGER NOT NULL DEFAULT 1");
            AddPlcPointColumn(db, "bit_index", "ALTER TABLE biz_plc_points ADD COLUMN bit_index INTEGER");
            AddPlcPointColumn(db, "description", "ALTER TABLE biz_plc_points ADD COLUMN description TEXT NOT NULL DEFAULT ''");
            db.Ado.ExecuteCommand("UPDATE biz_plc_points SET point_kind = 1 WHERE point_kind = 0");
        }

        /// <summary>
        /// 点位表缺列时补上。列名只来自本类的固定语句。
        /// </summary>
        private static void AddPlcPointColumn(SqlSugarScope db, string name, string sql)
        {
            var count = db.Ado.GetInt(
                "SELECT COUNT(1) FROM pragma_table_info('biz_plc_points') WHERE name = '" + name + "'");
            if (count > 0)
                return;

            db.Ado.ExecuteCommand(sql);
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
