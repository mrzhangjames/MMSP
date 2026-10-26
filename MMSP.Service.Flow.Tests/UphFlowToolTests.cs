using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;
using MMSP.Service.Flow.Steps;
using SqlSugar;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// UPH 按变量类型判 OK 或 NG，并给当前小时记 1。
    /// </summary>
    public class UphFlowToolTests
    {
        /// <summary>
        /// UPH 只读入参，不写出参。
        /// </summary>
        [Fact]
        public void Uph_ReadsOneVariableAndWritesNoOutput()
        {
            Assert.True(ToolVars.HasInputs(FlowToolKind.Uph));
            Assert.False(ToolVars.HasOutput(FlowToolKind.Uph));
            var cfg = """{"Name":"记录产量","Variable":"合格","ValueType":"bool","Inputs":[{"Variable":"合格"}]}""";
            Assert.Equal(["合格"], ToolVars.Referenced(FlowToolKind.Uph, cfg).ToArray());
            Assert.Empty(ToolVars.OutputNames(FlowToolKind.Uph, cfg));
        }

        /// <summary>
        /// 类型对上且取值合法时才会去写表。测试库没打开，所以停在写表之前。
        /// </summary>
        [Theory]
        [InlineData("bool", FlowVarType.Bool, "true")]
        [InlineData("bool", FlowVarType.Bool, "FALSE")]
        [InlineData("int", FlowVarType.Int, "1")]
        [InlineData("int", FlowVarType.Int, "0")]
        [InlineData("", FlowVarType.Bool, "true")]
        public void Uph_AcceptedValue_StopsWhenDatabaseIsClosed(string valueType, string declared, string value)
        {
            var result = Run(valueType, declared, value);
            Assert.Equal(FlowStepStatus.Failed, result.Status);
            Assert.Equal("客户库未打开", result.Text);
        }

        /// <summary>
        /// 变量没声明、值为空、类型对不上或取值认不出时不写表。
        /// </summary>
        [Fact]
        public void Uph_RejectsBeforeWrite()
        {
            Assert.Equal(
                "变量未声明",
                new UphFlowTool("""{"Variable":"合格","ValueType":"bool"}""")
                    .Execute(Context(Vars(("条码", "true", FlowVarType.Bool))))
                    .Text);
            Assert.Equal("值为空", Run("bool", FlowVarType.Bool, "  ").Text);
            Assert.Equal("类型不一致", Run("bool", FlowVarType.Int, "1").Text);
            Assert.Equal("类型不一致", Run("int", FlowVarType.Bool, "true").Text);
            Assert.Equal("类型不一致", Run("int", FlowVarType.String, "1").Text);
            Assert.Equal("判定无法识别", Run("bool", FlowVarType.Bool, "1").Text);
            Assert.Equal("判定无法识别", Run("int", FlowVarType.Int, "2").Text);
            Assert.Equal("判定无法识别", Run("int", FlowVarType.Int, "01").Text);
            Assert.Equal("配置无效", new UphFlowTool("""{"Variable":"合格","ValueType":"double"}""").Execute(Context(Vars(("合格", "true", FlowVarType.Bool)))).Text);
            Assert.Equal("配置无效", new UphFlowTool("{}").Execute(Context(Vars(("合格", "true", FlowVarType.Bool)))).Text);
        }

        /// <summary>
        /// 停止时不写表。
        /// </summary>
        [Fact]
        public void Uph_Cancel_DoesNotWrite()
        {
            var tool = new UphFlowTool("""{"Variable":"合格","ValueType":"bool"}""");
            var result = tool.Execute(new FlowStepContext(new CancellationToken(true), new FakeDevices(), Vars(("合格", "true", FlowVarType.Bool))));
            Assert.Equal(FlowStepStatus.Cancelled, result.Status);
        }

        /// <summary>
        /// 同一小时累加。OK 加良品，NG 加不良。累计产量含本小时，更早小时不改。
        /// </summary>
        [Fact]
        public void Uph_AddsOneToTheCurrentHour()
        {
            using var db = new TempDb();
            var eight = new DateTime(2026, 10, 26, 8, 15, 0);
            var nine = new DateTime(2026, 10, 26, 9, 0, 0);
            var nextDay = new DateTime(2026, 10, 27, 0, 5, 0);

            Assert.Null(UphHour.Add(db.Scope, eight, true));
            Assert.Null(UphHour.Add(db.Scope, eight, false));
            var first = Row(db.Scope, eight);
            Assert.Equal(2, first.HourOutput);
            Assert.Equal(1, first.GoodCount);
            Assert.Equal(1, first.DefectCount);
            Assert.Equal(2, first.TotalOutput);

            Assert.Null(UphHour.Add(db.Scope, nine, true));
            first = Row(db.Scope, eight);
            var second = Row(db.Scope, nine);
            Assert.Equal(2, first.TotalOutput);
            Assert.Equal(1, second.HourOutput);
            Assert.Equal(1, second.GoodCount);
            Assert.Equal(0, second.DefectCount);
            Assert.Equal(3, second.TotalOutput);

            Assert.Null(UphHour.Add(db.Scope, nextDay, false));
            second = Row(db.Scope, nine);
            var third = Row(db.Scope, nextDay);
            Assert.Equal(3, second.TotalOutput);
            Assert.Equal(1, third.HourOutput);
            Assert.Equal(0, third.GoodCount);
            Assert.Equal(1, third.DefectCount);
            Assert.Equal(1, third.TotalOutput);
            Assert.Equal(3, db.Scope.Queryable<UphEntity>().Count());
        }

        /// <summary>
        /// 中间缺了的小时不单独补行。累计产量只加已经有的小时产量。
        /// </summary>
        [Fact]
        public void Uph_SkipsMissingHoursWhenSumming()
        {
            using var db = new TempDb();
            Assert.Null(UphHour.Add(db.Scope, new DateTime(2026, 10, 26, 8, 0, 0), true));
            Assert.Null(UphHour.Add(db.Scope, new DateTime(2026, 10, 26, 8, 40, 0), true));
            Assert.Null(UphHour.Add(db.Scope, new DateTime(2026, 10, 26, 10, 0, 0), true));

            Assert.Equal(2, Row(db.Scope, new DateTime(2026, 10, 26, 8, 0, 0)).TotalOutput);
            Assert.Equal(3, Row(db.Scope, new DateTime(2026, 10, 26, 10, 0, 0)).TotalOutput);
            Assert.Equal(2, db.Scope.Queryable<UphEntity>().Count());
        }

        /// <summary>
        /// 跑一步。变量名固定为合格。
        /// </summary>
        private static FlowStepResult Run(string valueType, string declared, string value, string name = "合格")
        {
            var type = valueType.Length == 0 ? "" : $"""
                ,"ValueType":"{valueType}"
                """;
            var cfg = "{\"Variable\":\"" + name + "\"" + type + "}";
            return new UphFlowTool(cfg).Execute(Context(Vars((name, value, declared))));
        }

        /// <summary>
        /// 带类型的变量表。
        /// </summary>
        private static FlowVariables Vars(params (string Name, string Value, string Type)[] rows)
        {
            return FlowVariables.FromSeeds(rows);
        }

        /// <summary>
        /// 取出该小时那一行。
        /// </summary>
        private static UphEntity Row(SqlSugarScope db, DateTime time)
        {
            var hour = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0, DateTimeKind.Unspecified);
            return db.Queryable<UphEntity>().ToList().Single(item => item.HourTime == hour);
        }

        /// <summary>
        /// 没有设备的一步。
        /// </summary>
        private static FlowStepContext Context(FlowVariables variables)
        {
            return new FlowStepContext(CancellationToken.None, new FakeDevices(), variables);
        }

        /// <summary>
        /// 只建 UPH 表的临时库。
        /// </summary>
        private sealed class TempDb : IDisposable
        {
            /// <summary>
            /// 临时库文件。
            /// </summary>
            private readonly string _path;

            /// <summary>
            /// 打开空库并建小时表。
            /// </summary>
            public TempDb()
            {
                _path = Path.Combine(Path.GetTempPath(), "mmsp-uph-" + Guid.NewGuid().ToString("N") + ".db");
                Scope = new SqlSugarScope(new ConnectionConfig
                {
                    DbType = DbType.Sqlite,
                    ConnectionString = "Data Source=" + _path,
                    IsAutoCloseConnection = true
                });
                Scope.DbMaintenance.CreateDatabase();
                Scope.CodeFirst.InitTables(typeof(UphEntity));
                Scope.Ado.ExecuteCommand("CREATE UNIQUE INDEX IF NOT EXISTS idx_biz_uph_hour_time ON biz_uph (hour_time);");
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
