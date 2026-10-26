using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow.Tests
{
    /// <summary>
    /// 全部流程配置 JSON 的写出和校验。
    /// </summary>
    public class FlowFileTests
    {
        /// <summary>
        /// 写出后再读回，流程、步骤和变量都在，MES 字段不出现。
        /// </summary>
        [Fact]
        public void Write_RoundTrip_KeepsFlowsStepsAndVars()
        {
            var flowId = Guid.NewGuid();
            var json = FlowFile.Write(
                [Flow(flowId, "进站流程", 2, enabled: true, log: true)],
                [Step(flowId, FlowToolKind.PlcRead, "读码", "{\"address\":\"D1\"}", 1)],
                [Var(flowId, "条码", FlowVarType.String, "SN", 1)]);

            Assert.DoesNotContain("mes", json, StringComparison.OrdinalIgnoreCase);
            Assert.True(FlowFile.TryRead(json, out var flows, out var error), error);
            var flow = Assert.Single(flows);
            Assert.Equal("进站流程", flow.Name);
            Assert.True(flow.IsEnabled);
            Assert.True(flow.LogOutput);
            Assert.Equal(2, flow.SortIndex);
            var step = Assert.Single(flow.Steps);
            Assert.Equal(FlowToolKind.PlcRead, step.ToolKind);
            Assert.Equal("读码", step.Title);
            Assert.Equal("{\"address\":\"D1\"}", step.CfgValue);
            var variable = Assert.Single(flow.Vars);
            Assert.Equal("条码", variable.Name);
            Assert.Equal(FlowVarType.String, variable.DataType);
            Assert.Equal("SN", variable.InitialValue);
        }

        /// <summary>
        /// 空名单是一份合法文件，导入时会清空流程。
        /// </summary>
        [Fact]
        public void TryRead_EmptyFlows_IsValid()
        {
            Assert.True(FlowFile.TryRead("{\"version\":1,\"flows\":[]}", out var flows, out var error), error);
            Assert.Empty(flows);
        }

        /// <summary>
        /// 版本不对、工具种类不对、名称空、变量重复、带 MES 字段时都不通过。
        /// </summary>
        [Fact]
        public void TryRead_RejectsInvalidFiles()
        {
            Assert.False(FlowFile.TryRead("{\"version\":2,\"flows\":[]}", out _, out var version));
            Assert.Equal("版本不受支持", version);

            var badTool = Sample().Replace("\"toolKind\": 2", "\"toolKind\": 99", StringComparison.Ordinal);
            Assert.False(FlowFile.TryRead(badTool, out _, out var tool));
            Assert.Contains("工具种类无效", tool, StringComparison.Ordinal);

            var emptyName = Sample().Replace("\"进站流程\"", "\"  \"", StringComparison.Ordinal);
            Assert.False(FlowFile.TryRead(emptyName, out _, out var name));
            Assert.Contains("请填写流程名称", name, StringComparison.Ordinal);

            var duplicate = Sample().Replace(
                "\"vars\": []",
                """
                "vars": [
                  {"name":"条码","dataType":"string","initialValue":"","sortIndex":1},
                  {"name":"条码","dataType":"Int","initialValue":"1","sortIndex":2}
                ]
                """,
                StringComparison.Ordinal);
            Assert.False(FlowFile.TryRead(duplicate, out _, out var vars));
            Assert.Contains("名称重复", vars, StringComparison.Ordinal);

            var withMes = Sample().Replace("\"version\": 1", "\"version\": 1, \"mes\": {}", StringComparison.Ordinal);
            Assert.False(FlowFile.TryRead(withMes, out _, out var mes));
            Assert.Equal("文件含有不能导入的内容", mes);
        }

        /// <summary>
        /// 入库行使用新的 Id，步骤和变量挂到新的流程上。
        /// </summary>
        [Fact]
        public void CreateRows_AssignsNewIds()
        {
            Assert.True(FlowFile.TryRead(Sample(), out var flows, out var error), error);
            FlowFile.CreateRows(flows, out var flowRows, out var stepRows, out var varRows);
            var flow = Assert.Single(flowRows);
            var step = Assert.Single(stepRows);
            Assert.NotEqual(Guid.Empty, flow.Id);
            Assert.NotEqual(Guid.Empty, step.Id);
            Assert.Equal(flow.Id, step.FlowId);
            Assert.Empty(varRows);
            Assert.Equal(FlowToolKind.PlcRead, step.ToolKind);
        }

        /// <summary>
        /// 一份最小的合法文件。
        /// </summary>
        private static string Sample()
        {
            var id = Guid.NewGuid();
            return FlowFile.Write(
                [Flow(id, "进站流程", 1, true, false)],
                [Step(id, FlowToolKind.PlcRead, "读码", "", 1)],
                []);
        }

        /// <summary>
        /// 一条流程。
        /// </summary>
        private static FlowEntity Flow(Guid id, string name, int sort, bool enabled, bool log)
        {
            return new FlowEntity
            {
                Id = id,
                Name = name,
                SortIndex = sort,
                IsEnabled = enabled,
                LogOutput = log
            };
        }

        /// <summary>
        /// 一个流程步。
        /// </summary>
        private static FlowStepEntity Step(Guid flowId, FlowToolKind kind, string title, string cfg, int sort)
        {
            return new FlowStepEntity
            {
                Id = Guid.NewGuid(),
                FlowId = flowId,
                ToolKind = kind,
                Title = title,
                CfgValue = cfg,
                SortIndex = sort
            };
        }

        /// <summary>
        /// 一个变量。
        /// </summary>
        private static FlowVarEntity Var(Guid flowId, string name, string type, string initial, int sort)
        {
            return new FlowVarEntity
            {
                Id = Guid.NewGuid(),
                FlowId = flowId,
                Name = name,
                DataType = type,
                InitialValue = initial,
                SortIndex = sort
            };
        }
    }
}
