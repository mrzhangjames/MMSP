using System.Text.Json;
using System.Windows.Controls;
using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;
using MMSP.Plugin.Tool.Views;

namespace MMSP.Plugin.Tool
{
    /// <summary>
    /// 工具种类和用户控件的对照。流程页按种类创建控件，并读写配置。
    /// </summary>
    public static class ToolCatalog
    {
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// 按种类创建配置控件。cfg 为空时使用空配置。scanners 是设备里的扫码枪名称。
        /// </summary>
        public static UserControl Create(FlowToolKind kind, string cfg, IReadOnlyList<string>? scanners = null)
        {
            var names = scanners ?? [];
            return kind switch
            {
                FlowToolKind.SingleScan => Load(new SingleScanToolView(), Read<SingleScanToolEntity>(cfg), names),
                FlowToolKind.MultiScan => Load(new MultiScanToolView(), Read<MultiScanToolEntity>(cfg), names),
                FlowToolKind.PlcRead => Load(new PlcReadToolView(), Read<PlcReadToolEntity>(cfg), names),
                FlowToolKind.PlcWrite => Load(new PlcWriteToolView(), Read<PlcWriteToolEntity>(cfg), names),
                FlowToolKind.PlcMonitor => Load(new PlcMonitorToolView(), Read<PlcMonitorToolEntity>(cfg), names),
                FlowToolKind.Delay => Load(new DelayToolView(), Read<DelayToolEntity>(cfg), names),
                FlowToolKind.JsonParse => Load(new JsonParseToolView(), Read<JsonParseToolEntity>(cfg), names),
                FlowToolKind.Condition => Load(new ConditionToolView(), Read<ConditionToolEntity>(cfg), names),
                FlowToolKind.Script => Load(new ScriptToolView(), Read<ScriptToolEntity>(cfg), names),
                _ => new UserControl()
            };
        }

        /// <summary>
        /// 读出控件里的工具配置。
        /// </summary>
        public static string Config(UserControl view)
        {
            if (view.DataContext?.GetType().GetProperty("EditedTool")?.GetValue(view.DataContext) is not PluginToolModel tool)
                return "";

            return JsonSerializer.Serialize(tool, tool.GetType(), Json);
        }

        /// <summary>
        /// 步骤卡片上的说明。
        /// </summary>
        public static string Detail(FlowToolKind kind, string cfg)
        {
            return kind switch
            {
                FlowToolKind.SingleScan => ScanText(Read<SingleScanToolEntity>(cfg).Scanner, Read<SingleScanToolEntity>(cfg).Trigger, 0),
                FlowToolKind.MultiScan => ScanText(Read<MultiScanToolEntity>(cfg).Scanner, Read<MultiScanToolEntity>(cfg).Trigger, Read<MultiScanToolEntity>(cfg).Count),
                FlowToolKind.PlcRead => ReadText(Read<PlcReadToolEntity>(cfg)),
                FlowToolKind.PlcWrite => WriteText(Read<PlcWriteToolEntity>(cfg).Address, Read<PlcWriteToolEntity>(cfg).Value, Read<PlcWriteToolEntity>(cfg).DataType),
                FlowToolKind.PlcMonitor => MonitorText(Read<PlcMonitorToolEntity>(cfg)),
                FlowToolKind.Delay => DelayText(Read<DelayToolEntity>(cfg).Milliseconds),
                FlowToolKind.JsonParse => Join(Read<JsonParseToolEntity>(cfg).Source, Read<JsonParseToolEntity>(cfg).JsonPath, Read<JsonParseToolEntity>(cfg).Target),
                FlowToolKind.Condition => Read<ConditionToolEntity>(cfg).Expression.Trim(),
                FlowToolKind.Script => Short(Read<ScriptToolEntity>(cfg).Script),
                _ => ""
            };
        }

        /// <summary>
        /// 步骤标题。工具名称为空时用种类名称。
        /// </summary>
        public static string Title(FlowToolKind kind, string cfg, string fallback)
        {
            var name = NameOf(kind, cfg);
            return string.IsNullOrWhiteSpace(name) ? fallback : name.Trim();
        }

        /// <summary>
        /// 延时毫秒。其它工具为 0。
        /// </summary>
        public static int NumberValue(FlowToolKind kind, string cfg)
        {
            return kind == FlowToolKind.Delay ? Read<DelayToolEntity>(cfg).Milliseconds : 0;
        }

        /// <summary>
        /// 把已有配置装进控件。控件构造时已经建过一份空配置，这里换掉它。扫码页同时带上扫码枪列表。
        /// </summary>
        private static UserControl Load<TTool>(UserControl view, TTool tool, IReadOnlyList<string> scanners)
            where TTool : PluginToolModel
        {
            switch (view)
            {
                case SingleScanToolView single when tool is SingleScanToolEntity scan:
                    single.Load(scan, scanners);
                    break;
                case MultiScanToolView multi when tool is MultiScanToolEntity scan:
                    multi.Load(scan, scanners);
                    break;
                case PlcReadToolView read when tool is PlcReadToolEntity plc:
                    read.Load(plc);
                    break;
                case PlcWriteToolView write when tool is PlcWriteToolEntity plc:
                    write.Load(plc);
                    break;
                case PlcMonitorToolView monitor when tool is PlcMonitorToolEntity plc:
                    monitor.Load(plc);
                    break;
                case DelayToolView delay when tool is DelayToolEntity value:
                    delay.Load(value);
                    break;
                case JsonParseToolView json when tool is JsonParseToolEntity value:
                    json.Load(value);
                    break;
                case ConditionToolView condition when tool is ConditionToolEntity value:
                    condition.Load(value);
                    break;
                case ScriptToolView script when tool is ScriptToolEntity value:
                    script.Load(value);
                    break;
            }

            return view;
        }

        /// <summary>
        /// 从 Json 读出工具。空白或无法解析时返回空配置。
        /// </summary>
        private static TTool Read<TTool>(string cfg)
            where TTool : PluginToolModel, new()
        {
            if (string.IsNullOrWhiteSpace(cfg))
                return new TTool();

            try
            {
                return JsonSerializer.Deserialize<TTool>(cfg, Json) ?? new TTool();
            }
            catch (JsonException)
            {
                return new TTool();
            }
        }

        /// <summary>
        /// 工具名称。
        /// </summary>
        private static string NameOf(FlowToolKind kind, string cfg)
        {
            return kind switch
            {
                FlowToolKind.SingleScan => Read<SingleScanToolEntity>(cfg).Name,
                FlowToolKind.MultiScan => Read<MultiScanToolEntity>(cfg).Name,
                FlowToolKind.PlcRead => Read<PlcReadToolEntity>(cfg).Name,
                FlowToolKind.PlcWrite => Read<PlcWriteToolEntity>(cfg).Name,
                FlowToolKind.PlcMonitor => Read<PlcMonitorToolEntity>(cfg).Name,
                FlowToolKind.Delay => Read<DelayToolEntity>(cfg).Name,
                FlowToolKind.JsonParse => Read<JsonParseToolEntity>(cfg).Name,
                FlowToolKind.Condition => Read<ConditionToolEntity>(cfg).Name,
                FlowToolKind.Script => Read<ScriptToolEntity>(cfg).Name,
                _ => ""
            };
        }

        /// <summary>
        /// 扫码说明。数量大于 0 时是多扫码。
        /// </summary>
        private static string ScanText(string scanner, string trigger, int count)
        {
            var triggerText = string.IsNullOrWhiteSpace(trigger) ? "" : "触发字符串 " + trigger.Trim();
            var countText = count > 0 ? count + " 个" : "";
            return Join(scanner, triggerText, countText);
        }

        /// <summary>
        /// PLC 读取说明。
        /// </summary>
        private static string ReadText(PlcReadToolEntity tool)
        {
            var length = tool.Length > 0 ? "长度 " + tool.Length : "";
            return Join(tool.Address, tool.DataType.ToString(), length);
        }

        /// <summary>
        /// PLC 写入说明。
        /// </summary>
        private static string WriteText(string address, string value, PlcDataType dataType)
        {
            var assignment = string.IsNullOrWhiteSpace(address) && string.IsNullOrWhiteSpace(value)
                ? ""
                : address.Trim() + " = " + value.Trim();
            return Join(assignment, dataType.ToString());
        }

        /// <summary>
        /// PLC 监控说明。
        /// </summary>
        private static string MonitorText(PlcMonitorToolEntity tool)
        {
            var expect = string.IsNullOrWhiteSpace(tool.ExpectValue) ? "" : "期望 " + tool.ExpectValue.Trim();
            var reset = tool.Reset ? "重置" : "";
            var interval = tool.Interval > 0 ? "间隔 " + tool.Interval : "";
            var resetValue = tool.Reset && !string.IsNullOrWhiteSpace(tool.ResetValue) ? "重置值 " + tool.ResetValue.Trim() : "";
            return Join(tool.Address, tool.DataType.ToString(), expect, reset, interval, resetValue);
        }

        /// <summary>
        /// 延时说明。
        /// </summary>
        private static string DelayText(int milliseconds)
        {
            return milliseconds > 0 ? "超时 " + milliseconds + " ms" : "";
        }

        /// <summary>
        /// 用间隔号拼起非空片段。
        /// </summary>
        private static string Join(params string[] parts)
        {
            return string.Join(" · ", parts.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()));
        }

        /// <summary>
        /// 脚本只取第一行，避免卡片被整段脚本撑开。
        /// </summary>
        private static string Short(string text)
        {
            var line = text.Replace("\r", "").Split('\n')[0].Trim();
            return line.Length <= 40 ? line : line[..40];
        }
    }
}
