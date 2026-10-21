using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Service.Database;
using SqlSugar;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 旧流程步表删掉多余列时保留仍在用的数据。
    /// </summary>
    public class FlowStepSchemaTests
    {
        /// <summary>
        /// 和当前客户库一样的旧表。cfg_value 是后加的，允许空。
        /// </summary>
        private const string OldTableSql =
            """
            CREATE TABLE "biz_flow_steps"(
            "Id" uniqueidentifier NOT NULL PRIMARY KEY,
            "flow_id" uniqueidentifier NOT NULL,
            "sort_index" integer NOT NULL,
            "tool_kind" integer NOT NULL,
            "title" varchar(32) NOT NULL,
            "target" varchar(64) NOT NULL,
            "data_type" integer NOT NULL,
            "data_length" integer NOT NULL,
            "write_value" varchar(64) NOT NULL,
            "number_value" integer NOT NULL,
            "text_value" varchar(256) NOT NULL,
            "cfg_value" text)
            """;

        /// <summary>
        /// 旧列里的值和空配置都不进入新表。主键、流程、顺序、工具、标题和配置保留。
        /// </summary>
        [Fact]
        public void Trim_DropsLegacyColumnsAndKeepsRows()
        {
            using var db = new TempDb();
            db.Scope.Ado.ExecuteCommand(OldTableSql);
            db.Scope.Ado.ExecuteCommand(
                """
                INSERT INTO biz_flow_steps
                ("Id", flow_id, sort_index, tool_kind, title, target, data_type, data_length, write_value, number_value, text_value, cfg_value)
                VALUES
                ('3147242d-e515-43f1-a0cd-e2d5407f3674', '42ea3da0-f3de-446f-b52a-baf9eafcfe6b', 1, 5, '测试启动延时', 'x', 1, 2, 'w', 1000, 't', '{"Milliseconds":1000}'),
                ('b975a87e-0876-4a66-99d1-47b9a5f3efdf', '42ea3da0-f3de-446f-b52a-baf9eafcfe6b', 2, 8, '测试流程执行', '', 0, 0, '', 0, '', NULL)
                """);

            SqlSugarContext.TrimLegacyFlowSteps(db.Scope);
            SqlSugarContext.TrimLegacyFlowSteps(db.Scope);

            Assert.Equal(
                ["Id", "flow_id", "sort_index", "tool_kind", "title", "cfg_value"],
                ColumnNames(db.Scope));
            Assert.False(db.Scope.DbMaintenance.IsAnyTable("biz_flow_steps_legacy", false));

            var rows = db.Scope.Queryable<FlowStepEntity>().OrderBy(item => item.SortIndex).ToList();
            Assert.Equal(2, rows.Count);
            Assert.Equal(Guid.Parse("3147242d-e515-43f1-a0cd-e2d5407f3674"), rows[0].Id);
            Assert.Equal(Guid.Parse("42ea3da0-f3de-446f-b52a-baf9eafcfe6b"), rows[0].FlowId);
            Assert.Equal(1, rows[0].SortIndex);
            Assert.Equal(FlowToolKind.Delay, rows[0].ToolKind);
            Assert.Equal("测试启动延时", rows[0].Title);
            Assert.Equal("{\"Milliseconds\":1000}", rows[0].CfgValue);
            Assert.Equal(Guid.Parse("b975a87e-0876-4a66-99d1-47b9a5f3efdf"), rows[1].Id);
            Assert.Equal("", rows[1].CfgValue);
        }

        /// <summary>
        /// 还没有配置列的旧表先补空配置，再丢掉多余列。
        /// </summary>
        [Fact]
        public void Trim_AddsCfgValueWhenTheOldTableHasNone()
        {
            using var db = new TempDb();
            db.Scope.Ado.ExecuteCommand(
                """
                CREATE TABLE biz_flow_steps(
                "Id" uniqueidentifier NOT NULL PRIMARY KEY,
                flow_id uniqueidentifier NOT NULL,
                sort_index integer NOT NULL,
                tool_kind integer NOT NULL,
                title varchar(32) NOT NULL,
                number_value integer NOT NULL)
                """);
            db.Scope.Ado.ExecuteCommand(
                """
                INSERT INTO biz_flow_steps ("Id", flow_id, sort_index, tool_kind, title, number_value)
                VALUES ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', 1, 5, '延时', 1000)
                """);

            SqlSugarContext.TrimLegacyFlowSteps(db.Scope);

            var row = db.Scope.Queryable<FlowStepEntity>().ToList().Single();
            Assert.Equal("延时", row.Title);
            Assert.Equal("", row.CfgValue);
            Assert.Equal(FlowToolKind.Delay, row.ToolKind);
            Assert.DoesNotContain("number_value", ColumnNames(db.Scope));
        }

        /// <summary>
        /// 已经是当前结构时不改动原行。
        /// </summary>
        [Fact]
        public void Trim_LeavesCurrentTableUntouched()
        {
            using var db = new TempDb();
            db.Scope.CodeFirst.InitTables(typeof(FlowStepEntity));
            var id = Guid.Parse("33333333-3333-3333-3333-333333333333");
            db.Scope.Insertable(new FlowStepEntity
            {
                Id = id,
                FlowId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                SortIndex = 1,
                ToolKind = FlowToolKind.Delay,
                Title = "延时",
                CfgValue = "{\"Milliseconds\":50}"
            }).ExecuteCommand();

            SqlSugarContext.TrimLegacyFlowSteps(db.Scope);

            var row = db.Scope.Queryable<FlowStepEntity>().ToList().Single();
            Assert.Equal(id, row.Id);
            Assert.Equal("{\"Milliseconds\":50}", row.CfgValue);
            Assert.False(db.Scope.DbMaintenance.IsAnyTable("biz_flow_steps_legacy", false));
        }

        /// <summary>
        /// 读流程步表的列名，按定义顺序。
        /// </summary>
        private static List<string> ColumnNames(SqlSugarScope db)
        {
            var table = db.Ado.GetDataTable("SELECT name FROM pragma_table_info('biz_flow_steps') ORDER BY cid");
            var names = new List<string>(table.Rows.Count);
            foreach (System.Data.DataRow row in table.Rows)
                names.Add(row[0]?.ToString() ?? "");
            return names;
        }

        /// <summary>
        /// 用完即删的临时 SQLite。
        /// </summary>
        private sealed class TempDb : IDisposable
        {
            /// <summary>
            /// 临时库文件。
            /// </summary>
            private readonly string _path;

            /// <summary>
            /// 打开一个空的临时库。
            /// </summary>
            public TempDb()
            {
                _path = Path.Combine(Path.GetTempPath(), "mmsp-steps-" + Guid.NewGuid().ToString("N") + ".db");
                Scope = new SqlSugarScope(new ConnectionConfig
                {
                    DbType = SqlSugar.DbType.Sqlite,
                    ConnectionString = "Data Source=" + _path,
                    IsAutoCloseConnection = true
                });
                Scope.DbMaintenance.CreateDatabase();
            }

            /// <summary>
            /// 临时库连接。
            /// </summary>
            public SqlSugarScope Scope { get; }

            /// <summary>
            /// 关闭连接并删除临时文件。
            /// </summary>
            public void Dispose()
            {
                Scope.Dispose();
                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                foreach (var suffix in new[] { "", "-wal", "-shm" })
                {
                    var file = _path + suffix;
                    if (File.Exists(file))
                        File.Delete(file);
                }
            }
        }
    }
}
