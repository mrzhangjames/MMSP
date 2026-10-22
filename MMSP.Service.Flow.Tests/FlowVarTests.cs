using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;
using MMSP.Service.Device.Plc;
using MMSP.Service.Device.Scan;
using MMSP.Service.Flow;
using MMSP.Service.Flow.Steps;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 流程变量的保存规则，以及一步成功或失败时怎么写回变量表。
    /// </summary>
    public class FlowVarTests
    {
        /// <summary>
        /// 已被步骤使用的名称不能改名，也不能从名单里拿掉。
        /// </summary>
        [Fact]
        public void Rules_BoundName_RejectsRenameAndDelete()
        {
            var bound = new HashSet<string>(StringComparer.Ordinal) { "条码" };
            var renamed = Row("条码", "产品码", FlowVarType.String);
            var removed = Row("工位状态", "工位状态", FlowVarType.String);
            Assert.Equal("条码已被步骤使用", FlowVarRules.Check([renamed], bound));
            Assert.Equal("条码已被步骤使用", FlowVarRules.Check([removed], bound));
        }

        /// <summary>
        /// 已被使用的名称可以改数据类型和初始值。
        /// </summary>
        [Fact]
        public void Rules_BoundName_AllowsTypeAndInitial()
        {
            var bound = new HashSet<string>(StringComparer.Ordinal) { "条码" };
            var changed = new FlowVarDraft
            {
                OriginalName = "条码",
                Name = "条码",
                DataType = FlowVarType.Int,
                InitialValue = "0"
            };
            Assert.Equal("", FlowVarRules.Check([changed], bound));
        }

        /// <summary>
        /// 空名、超长、大括号、重复和非法类型都不能保存。
        /// </summary>
        [Fact]
        public void Rules_RejectsNameAndType()
        {
            var none = new HashSet<string>(StringComparer.Ordinal);
            Assert.Equal("请填写变量名称", FlowVarRules.Check([Row("", "  ", FlowVarType.String)], none));
            Assert.Equal("名称最长 32", FlowVarRules.Check([Row("", new string('名', 33), FlowVarType.String)], none));
            Assert.Equal("名称不能包含大括号", FlowVarRules.Check([Row("", "a{b}", FlowVarType.String)], none));
            Assert.Equal("名称重复", FlowVarRules.Check(
                [Row("", "条码", FlowVarType.String), Row("", "条码", FlowVarType.Int)],
                none));
            Assert.Equal("数据类型无效", FlowVarRules.Check([Row("", "条码", "Int16")], none));
        }

        /// <summary>
        /// 同一张变量表跨两步保留。后一步失败不覆盖前一步写下的值。
        /// </summary>
        [Fact]
        public void Values_StayAcrossSteps_FailureDoesNotOverwrite()
        {
            var variables = FlowVariables.FromSeeds([("工位状态", "9"), ("条码", "")]);
            var devices = new FakeDevices();
            devices.ReadResults.Enqueue(PlcIoResult.Ok("1"));
            var context = Context(devices, variables);
            var read = new PlcReadFlowTool("""{"Address":"D100","DataType":0,"Length":1,"Output":"工位状态"}""")
                .Execute(context);
            Assert.Equal(FlowStepStatus.Ok, read.Status);
            Assert.Equal("1", variables.Get("工位状态"));

            var failed = new PlcReadFlowTool("""{"Address":"D100","Output":"工位状态"}""").Execute(context);
            Assert.Equal(FlowStepStatus.Failed, failed.Status);
            Assert.Equal("1", variables.Get("工位状态"));
            Assert.Equal("", variables.Get("条码"));
        }

        /// <summary>
        /// 出参没在名单里时这一步失败，已有初始值不动。
        /// </summary>
        [Fact]
        public void UndeclaredOutput_FailsAndKeepsInitial()
        {
            var variables = FlowVariables.FromSeeds([("工位状态", "9")]);
            var devices = new FakeDevices();
            devices.ReadResults.Enqueue(PlcIoResult.Ok("1"));
            var result = new PlcReadFlowTool("""{"Address":"D100","Output":"别的"}""")
                .Execute(Context(devices, variables));
            Assert.Equal("变量未声明", result.Text);
            Assert.Equal("9", variables.Get("工位状态"));
            Assert.Equal("", variables.Get("别的"));
        }

        /// <summary>
        /// 单扫码勾选写入 PLC 后，先写条码，再把同一条码存入出参。
        /// </summary>
        [Fact]
        public void SingleScan_WritePlc_StoresAfterWrite()
        {
            var variables = FlowVariables.FromSeeds([("条码", "")]);
            var devices = new FakeDevices();
            devices.ScanResults.Enqueue(ScanIoResult.Ok("BC001"));
            var cfg = """{"Scanner":"Gun","Trigger":"LON","Output":"条码","WritePlc":true,"WriteAddress":"D200","WriteDataType":3,"WriteLength":32}""";
            var result = new SingleScanFlowTool(cfg).Execute(Context(devices, variables));
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal(("D200", PlcDataType.String, "BC001"), devices.Writes[0]);
            Assert.Equal("BC001", variables.Get("条码"));
        }

        /// <summary>
        /// PLC 写入失败时不把条码写入出参。
        /// </summary>
        [Fact]
        public void SingleScan_WritePlcFails_DoesNotStore()
        {
            var variables = FlowVariables.FromSeeds([("条码", "")]);
            var devices = new FakeDevices();
            devices.ScanResults.Enqueue(ScanIoResult.Ok("BC001"));
            devices.WriteResults.Enqueue(PlcIoResult.Fail("通讯失败"));
            var cfg = """{"Scanner":"Gun","Trigger":"LON","Output":"条码","WritePlc":true,"WriteAddress":"D200","WriteDataType":3}""";
            var result = new SingleScanFlowTool(cfg).Execute(Context(devices, variables));
            Assert.Equal(FlowStepStatus.Failed, result.Status);
            Assert.Equal("通讯失败", result.Text);
            Assert.Equal("", variables.Get("条码"));
        }

        /// <summary>
        /// 脚本引用了未声明的入参时失败。声明了则仍返回正文。
        /// </summary>
        [Fact]
        public void Script_RequireInputs()
        {
            var cfg = """{"Name":"记录","Script":"正文","Inputs":[{"Name":"脚本","Variable":"条码"}]}""";
            var missing = new ScriptFlowTool(cfg).Execute(Context(new FakeDevices(), FlowVariables.Empty));
            Assert.Equal("变量未声明", missing.Text);

            var variables = FlowVariables.FromSeeds([("条码", "BC001")]);
            var result = new ScriptFlowTool(cfg).Execute(Context(new FakeDevices(), variables));
            Assert.Equal(FlowStepStatus.Ok, result.Status);
            Assert.Equal("正文", result.Text);
        }

        /// <summary>
        /// 做一个带变量表的执行参数。
        /// </summary>
        private static FlowStepContext Context(FakeDevices devices, FlowVariables variables)
        {
            return new FlowStepContext(CancellationToken.None, devices, variables);
        }

        /// <summary>
        /// 做一行草稿。原名和新名分开，用来测改名。
        /// </summary>
        private static FlowVarDraft Row(string original, string name, string dataType)
        {
            return new FlowVarDraft
            {
                OriginalName = original,
                Name = name,
                DataType = dataType,
                InitialValue = ""
            };
        }
    }
}
