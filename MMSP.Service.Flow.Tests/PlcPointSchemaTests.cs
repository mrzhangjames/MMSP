using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Service.Database;
using SqlSugar;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 点位类型、Bit 位和描述的入库规则。旧点位表要补上这三列。
    /// </summary>
    public class PlcPointSchemaTests
    {
        /// <summary>
        /// 16 位整数保留 0 到 15，32 位整数保留 0 到 31。越界、浮点、字符串和长度不是 1 时为空。
        /// </summary>
        [Fact]
        public void NormalizeBitIndex_KeepsOnlyASingleIntegerBit()
        {
            Assert.Equal(0, PlcPointEntity.NormalizeBitIndex(PlcDataType.Int16, 1, 0));
            Assert.Equal(15, PlcPointEntity.NormalizeBitIndex(PlcDataType.Int16, 1, 15));
            Assert.Equal(15, PlcPointEntity.NormalizeBitIndex(PlcDataType.UInt16, 1, 15));
            Assert.Null(PlcPointEntity.NormalizeBitIndex(PlcDataType.Int16, 1, 16));
            Assert.Null(PlcPointEntity.NormalizeBitIndex(PlcDataType.UInt16, 1, -1));
            Assert.Equal(31, PlcPointEntity.NormalizeBitIndex(PlcDataType.Int32, 1, 31));
            Assert.Null(PlcPointEntity.NormalizeBitIndex(PlcDataType.Int32, 1, 32));
            Assert.Null(PlcPointEntity.NormalizeBitIndex(PlcDataType.Float, 1, 0));
            Assert.Null(PlcPointEntity.NormalizeBitIndex(PlcDataType.String, 1, 0));
            Assert.Null(PlcPointEntity.NormalizeBitIndex(PlcDataType.Int16, 2, 0));
            Assert.Null(PlcPointEntity.NormalizeBitIndex(PlcDataType.Int32, 1, null));
        }

        /// <summary>
        /// 未定义的类型收成流程。描述去掉两端空白并截到 256。
        /// </summary>
        [Fact]
        public void NormalizeKindAndDescription_UsesFlowAndCapsText()
        {
            Assert.Equal(PlcPointKind.Flow, PlcPointEntity.NormalizeKind((PlcPointKind)9));
            Assert.Equal(PlcPointKind.Flow, PlcPointEntity.NormalizeKind((PlcPointKind)0));
            Assert.Equal(PlcPointKind.Heartbeat, PlcPointEntity.NormalizeKind(PlcPointKind.Heartbeat));
            Assert.Equal(PlcPointKind.Status, PlcPointEntity.NormalizeKind(PlcPointKind.Status));
            Assert.Equal(PlcPointKind.Monitor, PlcPointEntity.NormalizeKind(PlcPointKind.Monitor));
            Assert.Equal(PlcPointKind.Heartbeat, PlcPointEntity.ExtraSingleton(
                [PlcPointKind.Heartbeat, PlcPointKind.Flow, PlcPointKind.Heartbeat]));
            Assert.Equal(PlcPointKind.Status, PlcPointEntity.ExtraSingleton(
                [PlcPointKind.Status, PlcPointKind.Status]));
            Assert.Null(PlcPointEntity.ExtraSingleton(
                [PlcPointKind.Flow, PlcPointKind.Monitor, PlcPointKind.Heartbeat, PlcPointKind.Status]));
            Assert.Equal(PlcStatusValue.Run, (PlcStatusValue)0);
            Assert.Equal(PlcStatusValue.Pause, (PlcStatusValue)1);
            Assert.Equal(PlcStatusValue.EStop, (PlcStatusValue)2);
            Assert.Equal(PlcStatusValue.Debug, (PlcStatusValue)3);
            Assert.Equal(PlcStatusValue.Manual, (PlcStatusValue)4);
            Assert.Equal("工位完成", PlcPointEntity.NormalizeDescription("  工位完成  "));
            Assert.Equal("", PlcPointEntity.NormalizeDescription(null));
            Assert.Equal(PlcPointEntity.DescriptionMaxLength, PlcPointEntity.NormalizeDescription(new string('测', 300)).Length);
        }

        /// <summary>
        /// 旧点位表补列后，原行类型是流程，Bit 位和描述为空，名称仍在。
        /// </summary>
        [Fact]
        public void EnsurePlcPointColumns_FillsExistingRows()
        {
            using var db = new TempDb();
            db.Scope.Ado.ExecuteCommand(
                """
                CREATE TABLE biz_plc_points(
                "Id" uniqueidentifier NOT NULL PRIMARY KEY,
                name varchar(64) NOT NULL,
                address varchar(64) NOT NULL,
                data_type integer NOT NULL,
                data_length integer NOT NULL,
                access integer NOT NULL,
                is_valid integer NOT NULL,
                sort_index integer NOT NULL)
                """);
            db.Scope.Ado.ExecuteCommand(
                """
                INSERT INTO biz_plc_points
                ("Id", name, address, data_type, data_length, access, is_valid, sort_index)
                VALUES ('3147242d-e515-43f1-a0cd-e2d5407f3674', '工位状态', 'D100', 0, 1, 0, 1, 1)
                """);

            SqlSugarContext.EnsurePlcPointColumns(db.Scope);
            SqlSugarContext.EnsurePlcPointColumns(db.Scope);
            db.Scope.CodeFirst.InitTables(typeof(PlcPointEntity));

            var row = db.Scope.Queryable<PlcPointEntity>().ToList().Single();
            Assert.Equal(Guid.Parse("3147242d-e515-43f1-a0cd-e2d5407f3674"), row.Id);
            Assert.Equal("工位状态", row.Name);
            Assert.Equal("D100", row.Address);
            Assert.Equal(PlcPointKind.Flow, row.PointKind);
            Assert.Null(row.BitIndex);
            Assert.Equal("", row.Description);
        }

        /// <summary>
        /// 新表能写入心跳点和它的 Bit 位、描述。
        /// </summary>
        [Fact]
        public void NewTable_StoresKindBitAndDescription()
        {
            using var db = new TempDb();
            db.Scope.CodeFirst.InitTables(typeof(PlcPointEntity));
            var id = Guid.Parse("55555555-5555-5555-5555-555555555555");
            db.Scope.Insertable(new PlcPointEntity
            {
                Id = id,
                Name = "心跳",
                Address = "D10",
                DataType = PlcDataType.Int16,
                DataLength = 1,
                Access = PlcAccess.ReadOnly,
                IsValid = true,
                SortIndex = 1,
                PointKind = PlcPointKind.Heartbeat,
                BitIndex = 3,
                Description = "整机心跳"
            }).ExecuteCommand();

            var row = db.Scope.Queryable<PlcPointEntity>().ToList().Single();
            Assert.Equal(id, row.Id);
            Assert.Equal(PlcPointKind.Heartbeat, row.PointKind);
            Assert.Equal(3, row.BitIndex);
            Assert.Equal("整机心跳", row.Description);
        }

        /// <summary>
        /// 已经写成触发的旧行，打开库时改成流程。
        /// </summary>
        [Fact]
        public void EnsurePlcPointColumns_TurnsRetiredTriggerIntoFlow()
        {
            using var db = new TempDb();
            db.Scope.Ado.ExecuteCommand(
                """
                CREATE TABLE biz_plc_points(
                "Id" uniqueidentifier NOT NULL PRIMARY KEY,
                name varchar(64) NOT NULL,
                address varchar(64) NOT NULL,
                data_type integer NOT NULL,
                data_length integer NOT NULL,
                access integer NOT NULL,
                is_valid integer NOT NULL,
                sort_index integer NOT NULL,
                point_kind integer NOT NULL,
                bit_index integer,
                description text NOT NULL)
                """);
            db.Scope.Ado.ExecuteCommand(
                """
                INSERT INTO biz_plc_points
                ("Id", name, address, data_type, data_length, access, is_valid, sort_index, point_kind, bit_index, description)
                VALUES ('66666666-6666-6666-6666-666666666666', '进站完成', 'D10', 0, 1, 0, 1, 1, 0, NULL, '')
                """);

            SqlSugarContext.EnsurePlcPointColumns(db.Scope);

            var row = db.Scope.Queryable<PlcPointEntity>().ToList().Single();
            Assert.Equal(PlcPointKind.Flow, row.PointKind);
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
                _path = Path.Combine(Path.GetTempPath(), "mmsp-points-" + Guid.NewGuid().ToString("N") + ".db");
                Scope = new SqlSugarScope(new ConnectionConfig
                {
                    DbType = DbType.Sqlite,
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
