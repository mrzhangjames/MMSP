using System.Text.Json;
using MMSP.Common.Helper;
using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;
using MMSP.Service.Flow.Steps;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// CSV 按列追加一行。目录在临时数据根下，不写客户 Data\File。
    /// </summary>
    public class CsvFlowToolTests
    {
        /// <summary>
        /// 效果图里的日期。运行按传入的这一天生成文件名，不看本机今天。
        /// </summary>
        private static readonly DateTime Day = new(2026, 10, 27);

        /// <summary>
        /// CSV 只读各列的数据来源，不写出参。列名不算引用。
        /// </summary>
        [Fact]
        public void Csv_ReadsSourcesAndWritesNoOutput()
        {
            Assert.True(ToolVars.HasInputs(FlowToolKind.Csv));
            Assert.False(ToolVars.HasOutput(FlowToolKind.Csv));
            var cfg = """{"Name":"导出记录","Rows":[{"Column":"结果","Source":"合格"},{"Column":"条码","Source":"条码"},{"Column":"空列","Source":"  "}]}""";
            Assert.Equal(["合格", "条码"], ToolVars.Referenced(FlowToolKind.Csv, cfg).ToArray());
            Assert.Empty(ToolVars.OutputNames(FlowToolKind.Csv, cfg));
            Assert.IsType<CsvFlowTool>(FlowToolCatalog.Create(FlowToolKind.Csv, cfg));
        }

        /// <summary>
        /// 列、来源、流程名或文件名不合法时不建目录。
        /// </summary>
        [Fact]
        public void Csv_RejectsBeforeWrite()
        {
            var variables = Vars();
            Assert.Equal("配置无效", new CsvFlowTool("").Execute(Context("进站流程", variables)).Text);
            Assert.Equal("配置无效", new CsvFlowTool("{").Execute(Context("进站流程", variables)).Text);
            Assert.Equal("数据列表为空", Reject(Tool(), "进站流程", variables));
            Assert.Equal("流程名为空", Reject(Tool(("条码", "条码")), "  ", variables));
            Assert.Equal("流程名为空", Reject(Tool(), "", variables));
            Assert.Equal("列名为空", Reject(Tool(("  ", "条码")), "进站流程", variables));
            Assert.Equal("列名最长 32", Reject(Tool((new string('列', 33), "条码")), "进站流程", variables));
            Assert.Equal("列名无效", Reject(Tool(("列\n名", "条码")), "进站流程", variables));
            Assert.Equal("列名重复", Reject(Tool(("条码", "条码"), (" 条码 ", "合格")), "进站流程", variables));
            Assert.Equal("数据来源为空", Reject(Tool(("条码", "  ")), "进站流程", variables));
            Assert.Equal("变量未声明", Reject(Tool(("条码", "没有")), "进站流程", variables));
            Assert.Equal("文件名无效", Reject(Tool(false, "a/b.csv", ("条码", "条码")), "进站流程", variables));
        }

        /// <summary>
        /// 按日期生成时忽略配置里的旧文件名。逗号和空值各占一格，变量值不被改写。再追加一行时表头不变。
        /// </summary>
        [Fact]
        public void Append_WritesDatedFileAndAddsARow()
        {
            using var root = new TempRoot();
            var variables = Vars(("条码", " SN,1 "), ("产品码", ""), ("合格", "true"));
            var tool = Tool(true, "旧.csv", ("条码", "条码"), ("结果", "合格"), ("产品码", "产品码"));

            var error = CsvExport.Append(tool, "进站流程", variables, Day, root.Path, out var written);
            Assert.Equal("", error);
            Assert.Equal("进站流程20261027.csv", written);
            var path = Path.Combine(root.Path, "File", "进站流程", written);
            var rows = CsvFileHelper.Read(path);
            Assert.Equal(new[] { "条码", "结果", "产品码" }, rows[0]);
            Assert.Equal(new[] { " SN,1 ", "true", "" }, rows[1]);
            Assert.Equal(" SN,1 ", variables.Get("条码"));
            Assert.Equal("", variables.Get("产品码"));
            Assert.Equal("true", variables.Get("合格"));

            variables.Set("条码", "SN2");
            error = CsvExport.Append(tool, "进站流程", variables, Day, root.Path, out written);
            Assert.Equal("", error);
            rows = CsvFileHelper.Read(path);
            Assert.Equal(3, rows.Count);
            Assert.Equal(new[] { "条码", "结果", "产品码" }, rows[0]);
            Assert.Equal(new[] { "SN2", "true", "" }, rows[2]);
        }

        /// <summary>
        /// 自定义文件名补上 .csv。流程名里的斜杠不进入目录。
        /// </summary>
        [Fact]
        public void Append_CustomNameAndSanitizedFolder()
        {
            using var root = new TempRoot();
            var tool = Tool(false, "记录", ("条码", "条码"));
            var error = CsvExport.Append(tool, "进站/流程", Vars(), Day, root.Path, out var written);
            Assert.Equal("", error);
            Assert.Equal("记录.csv", written);
            Assert.Equal(@"Data\File\进站流程", CsvPaths.DisplayDirectory("进站/流程"));
            var path = Path.Combine(root.Path, "File", "进站流程", "记录.csv");
            Assert.True(File.Exists(path));
            Assert.False(Directory.Exists(Path.Combine(root.Path, "File", "进站")));
        }

        /// <summary>
        /// 已有表头和配置不一致时不改文件。
        /// </summary>
        [Fact]
        public void Append_HeaderMismatch_LeavesFile()
        {
            using var root = new TempRoot();
            var folder = Path.Combine(root.Path, "File", "进站流程");
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, "进站流程20261027.csv");
            CsvFileHelper.Write(path, new[] { new[] { "旧", "值" }, new[] { "a", "b" } });
            var before = File.ReadAllBytes(path);

            var error = CsvExport.Append(
                Tool(true, "", ("条码", "条码"), ("结果", "合格")),
                "进站流程",
                Vars(),
                Day,
                root.Path,
                out var written);

            Assert.Equal("表头不一致", error);
            Assert.Equal("", written);
            Assert.Equal(before, File.ReadAllBytes(path));
        }

        /// <summary>
        /// 目录建不出来时这一步失败。
        /// </summary>
        [Fact]
        public void Append_CannotCreateDirectory()
        {
            var blocked = Path.Combine(Path.GetTempPath(), "mmsp-csv-" + Guid.NewGuid().ToString("N"));
            File.WriteAllText(blocked, "x");
            try
            {
                var error = CsvExport.Append(Tool(("条码", "条码")), "进站流程", Vars(), Day, blocked, out var written);
                Assert.Equal("文件无法写入", error);
                Assert.Equal("", written);
            }
            finally
            {
                File.Delete(blocked);
            }
        }

        /// <summary>
        /// 已经取消时不建目录，也不写文件。
        /// </summary>
        [Fact]
        public void Execute_Cancel_CreatesNothing()
        {
            using var root = new TempRoot();
            var tool = new CsvFlowTool(Json(Tool(("条码", "条码"))));
            tool.DataRoot = root.Path;
            var result = tool.Execute(new FlowStepContext(new CancellationToken(true), new FakeDevices(), Vars(), "进站流程"));
            Assert.Equal(FlowStepStatus.Cancelled, result.Status);
            Assert.False(Directory.Exists(root.Path));
        }

        /// <summary>
        /// 执行时写到测试目录，成功文本只有文件名。
        /// </summary>
        [Fact]
        public void Execute_WritesUnderDataRoot()
        {
            using var root = new TempRoot();
            var tool = new CsvFlowTool(Json(Tool(true, "旧.csv", ("条码", "条码"))));
            tool.DataRoot = root.Path;
            var result = tool.Execute(Context("进站流程", Vars()));
            var expected = CsvPaths.DefaultFile("进站流程", DateTime.Now);
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal(expected, result.Text);
            Assert.True(File.Exists(Path.Combine(root.Path, "File", "进站流程", expected)));
        }

        /// <summary>
        /// 一轮开始时把流程名交给工具。CSV 用这个名字建目录。
        /// </summary>
        [Fact]
        public void Cycle_SetsFlowName()
        {
            var probe = new NameProbe();
            var flows = new[]
            {
                new RunnableFlow("进站流程", false, new[] { new RunnableStep(1, "导出", probe) })
            };
            FlowCycle.Execute(
                flows,
                new FlowStepContext(CancellationToken.None, new FakeDevices(), FlowVariables.Empty, "旧名"),
                _ => { },
                _ => { });
            Assert.Equal("进站流程", probe.FlowName);
        }

        /// <summary>
        /// 追加并确认没有写出文件。
        /// </summary>
        private static string Reject(CsvToolEntity tool, string? flow, FlowVariables variables)
        {
            using var root = new TempRoot();
            var error = CsvExport.Append(tool, flow, variables, Day, root.Path, out var written);
            Assert.Equal("", written);
            Assert.False(Directory.Exists(root.Path));
            return error;
        }

        /// <summary>
        /// 一份 CSV 配置。fromDate 为 true 时文件名只是预览。
        /// </summary>
        private static CsvToolEntity Tool(params (string Column, string Source)[] rows)
        {
            return Tool(true, "", rows);
        }

        /// <summary>
        /// 一份 CSV 配置。
        /// </summary>
        private static CsvToolEntity Tool(bool fromDate, string fileName, params (string Column, string Source)[] rows)
        {
            return new CsvToolEntity
            {
                Name = "导出记录",
                NameFromDate = fromDate,
                FileName = fileName,
                Rows = rows.Select(item => new CsvColumn { Column = item.Column, Source = item.Source }).ToList()
            };
        }

        /// <summary>
        /// 进站流程里用到的三个变量。
        /// </summary>
        private static FlowVariables Vars()
        {
            return Vars(("条码", "SN1"), ("产品码", ""), ("合格", "true"));
        }

        /// <summary>
        /// 按给定的值建变量表。
        /// </summary>
        private static FlowVariables Vars(params (string Name, string Value)[] rows)
        {
            return FlowVariables.FromSeeds(rows);
        }

        /// <summary>
        /// 没有设备的一步。
        /// </summary>
        private static FlowStepContext Context(string flowName, FlowVariables variables)
        {
            return new FlowStepContext(CancellationToken.None, new FakeDevices(), variables, flowName);
        }

        /// <summary>
        /// 配置写成工具执行时用的 Json。
        /// </summary>
        private static string Json(CsvToolEntity tool)
        {
            return JsonSerializer.Serialize(tool);
        }

        /// <summary>
        /// 临时数据根。用完删掉。
        /// </summary>
        private sealed class TempRoot : IDisposable
        {
            /// <summary>
            /// 创建一个还没建目录的临时路径。
            /// </summary>
            public TempRoot()
            {
                Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "mmsp-csv-" + Guid.NewGuid().ToString("N"));
            }

            /// <summary>
            /// 临时数据根。
            /// </summary>
            public string Path { get; }

            /// <summary>
            /// 删掉测试写出的目录。
            /// </summary>
            public void Dispose()
            {
                if (Directory.Exists(Path))
                    Directory.Delete(Path, true);
            }
        }

        /// <summary>
        /// 记下执行时看到的流程名。
        /// </summary>
        private sealed class NameProbe : FlowToolStep
        {
            /// <summary>
            /// 执行时的流程名。
            /// </summary>
            public string FlowName { get; private set; } = "";

            /// <summary>
            /// 只记录流程名。
            /// </summary>
            public override FlowStepResult Execute(FlowStepContext context)
            {
                FlowName = context.FlowName;
                return Ok("ok");
            }
        }
    }
}
