using System.Text.Json;
using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;

namespace MMSP.Core.Model.Tool
{
    /// <summary>
    /// 从工具配置里读出、写回流程变量名。变量表本身不存这些绑定。
    /// </summary>
    public static class ToolVars
    {
        /// <summary>
        /// 与属性窗口相同的 Json 选项。
        /// </summary>
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// 这种工具左侧列出流程变量，点选本步要读取的名称。
        /// </summary>
        public static bool HasInputs(FlowToolKind kind)
        {
            return kind is FlowToolKind.PlcWrite
                or FlowToolKind.JsonParse
                or FlowToolKind.Condition
                or FlowToolKind.Script
                or FlowToolKind.Uph;
        }

        /// <summary>
        /// 这种工具能否在右侧点选一个出参。
        /// </summary>
        public static bool HasOutput(FlowToolKind kind)
        {
            return kind is FlowToolKind.SingleScan
                or FlowToolKind.PlcRead
                or FlowToolKind.JsonParse
                or FlowToolKind.Condition;
        }

        /// <summary>
        /// 多扫码按枪数乘条码数量分别绑定出参。
        /// </summary>
        public static bool HasManyOutputs(FlowToolKind kind)
        {
            return kind == FlowToolKind.MultiScan;
        }

        /// <summary>
        /// 配置里已经绑定的变量名。空白名称略过。
        /// </summary>
        public static IEnumerable<string> Referenced(FlowToolKind kind, string cfg)
        {
            var tool = Read(kind, cfg);
            if (tool is null)
                yield break;

            foreach (var name in Names(tool, true))
            {
                var text = (name ?? "").Trim();
                if (text.Length > 0)
                    yield return text;
            }
        }

        /// <summary>
        /// 配置里作为出参写入的变量名。入参不算。
        /// </summary>
        public static IEnumerable<string> OutputNames(FlowToolKind kind, string cfg)
        {
            var tool = Read(kind, cfg);
            if (tool is null)
                yield break;

            foreach (var name in Names(tool, false))
            {
                var text = (name ?? "").Trim();
                if (text.Length > 0)
                    yield return text;
            }
        }

        /// <summary>
        /// 单出参名称。这种工具没有单出参时返回空。
        /// </summary>
        public static string Output(PluginToolModel tool)
        {
            return tool switch
            {
                SingleScanToolEntity scan => scan.Output ?? "",
                PlcReadToolEntity read => read.Output ?? "",
                ConditionToolEntity condition => condition.Output ?? "",
                JsonParseToolEntity json => json.Target ?? "",
                _ => ""
            };
        }

        /// <summary>
        /// 写回单出参。多扫码不走这里。
        /// </summary>
        public static void SetOutput(PluginToolModel tool, string name)
        {
            var text = name ?? "";
            switch (tool)
            {
                case SingleScanToolEntity scan:
                    scan.Output = text;
                    break;
                case PlcReadToolEntity read:
                    read.Output = text;
                    break;
                case ConditionToolEntity condition:
                    condition.Output = text;
                    break;
                case JsonParseToolEntity json:
                    json.Target = text;
                    break;
            }
        }

        /// <summary>
        /// 多扫码的出参槽。调用方按枪数乘条码数量读写。
        /// </summary>
        public static List<string> Slots(MultiScanToolEntity tool)
        {
            tool.Outputs ??= [];
            return tool.Outputs;
        }

        /// <summary>
        /// 多扫码出参槽个数。没选枪时按一把算，再乘每枪的条码数量。枪名按顿号拆开，空白段不算。
        /// </summary>
        public static int OutputSlotCount(MultiScanToolEntity tool)
        {
            ArgumentNullException.ThrowIfNull(tool);
            var guns = 0;
            if (!string.IsNullOrWhiteSpace(tool.Scanner))
            {
                guns = tool.Scanner.Split('、', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
            }

            if (guns == 0)
                guns = 1;

            var count = tool.Count < 0 ? 0 : tool.Count;
            return guns * count;
        }

        /// <summary>
        /// 把多扫码出参槽调整到指定个数。多出来的丢掉，不够的补空。
        /// </summary>
        public static void FitSlots(MultiScanToolEntity tool, int count)
        {
            var slots = Slots(tool);
            var size = count < 0 ? 0 : count;
            while (slots.Count > size)
                slots.RemoveAt(slots.Count - 1);

            while (slots.Count < size)
                slots.Add("");
        }

        /// <summary>
        /// 读出工具上的入参列表。没有入参的工具返回空列表，不新建。
        /// </summary>
        public static List<ToolInput>? Inputs(PluginToolModel tool)
        {
            return tool switch
            {
                PlcWriteToolEntity write => write.Inputs ??= [],
                JsonParseToolEntity json => json.Inputs ??= [],
                ConditionToolEntity condition => condition.Inputs ??= [],
                ScriptToolEntity script => script.Inputs ??= [],
                UphToolEntity uph => uph.Inputs ??= [],
                _ => null
            };
        }

        /// <summary>
        /// 按种类读配置。空白或损坏时返回空。
        /// </summary>
        private static PluginToolModel? Read(FlowToolKind kind, string cfg)
        {
            if (string.IsNullOrWhiteSpace(cfg))
                return null;

            try
            {
                return kind switch
                {
                    FlowToolKind.SingleScan => Deserialize<SingleScanToolEntity>(cfg),
                    FlowToolKind.MultiScan => Deserialize<MultiScanToolEntity>(cfg),
                    FlowToolKind.PlcRead => Deserialize<PlcReadToolEntity>(cfg),
                    FlowToolKind.PlcWrite => Deserialize<PlcWriteToolEntity>(cfg),
                    FlowToolKind.PlcMonitor => Deserialize<PlcMonitorToolEntity>(cfg),
                    FlowToolKind.JsonParse => Deserialize<JsonParseToolEntity>(cfg),
                    FlowToolKind.Condition => Deserialize<ConditionToolEntity>(cfg),
                    FlowToolKind.Script => Deserialize<ScriptToolEntity>(cfg),
                    FlowToolKind.Uph => Deserialize<UphToolEntity>(cfg),
                    _ => null
                };
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// 反序列化一份工具配置。
        /// </summary>
        private static TTool? Deserialize<TTool>(string cfg)
            where TTool : PluginToolModel
        {
            return JsonSerializer.Deserialize<TTool>(cfg, Json);
        }

        /// <summary>
        /// 一份工具上出现的变量名，含空白。
        /// </summary>
        private static IEnumerable<string> Names(PluginToolModel tool, bool includeInputs)
        {
            switch (tool)
            {
                case SingleScanToolEntity scan:
                    yield return scan.Output;
                    break;
                case MultiScanToolEntity multi:
                    if (multi.Outputs is null)
                        yield break;

                    foreach (var name in multi.Outputs)
                        yield return name;
                    break;
                case PlcReadToolEntity read:
                    yield return read.Output;
                    break;
                case PlcMonitorToolEntity:
                    yield break;
                case PlcWriteToolEntity write:
                    if (!includeInputs)
                        yield break;

                    if (write.UseVariable)
                        yield return write.Value ?? "";

                    if (write.Inputs is null)
                        yield break;

                    foreach (var input in write.Inputs)
                        yield return input?.Variable ?? "";
                    break;
                case JsonParseToolEntity json:
                    yield return json.Target;
                    if (!includeInputs)
                        yield break;

                    yield return json.Source ?? "";
                    if (json.Inputs is null)
                        yield break;

                    foreach (var input in json.Inputs)
                        yield return input?.Variable ?? "";
                    break;
                case ConditionToolEntity condition:
                    yield return condition.Output;
                    if (!includeInputs || condition.Inputs is null)
                        yield break;

                    foreach (var input in condition.Inputs)
                        yield return input?.Variable ?? "";
                    break;
                case ScriptToolEntity script:
                    if (!includeInputs || script.Inputs is null)
                        yield break;

                    foreach (var input in script.Inputs)
                        yield return input?.Variable ?? "";
                    break;
                case UphToolEntity uph:
                    if (!includeInputs)
                        yield break;

                    yield return uph.Variable ?? "";
                    break;
            }
        }
    }
}
