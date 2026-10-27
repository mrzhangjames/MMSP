using System.Globalization;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool.PluginLux;

namespace MMSP.Service.Flow.Steps.PluginLux
{
    /// <summary>
    /// 立讯报警日志。每轮读每一行，上升沿才写流程日志，不挡住后面的流程。
    /// </summary>
    public sealed class LuxAlarmLogFlowTool : FlowToolStep
    {
        /// <summary>
        /// 报警配置。解析失败时为空。
        /// </summary>
        private readonly LuxAlarmLogToolEntity? _tool;

        /// <summary>
        /// 解析失败的原因。成功时为空。
        /// </summary>
        private readonly string? _error;

        /// <summary>
        /// 上升沿日志。
        /// </summary>
        private readonly Action<string> _info;

        /// <summary>
        /// 单行读取失败日志。不让这一步失败。
        /// </summary>
        private readonly Action<string> _errorLog;

        /// <summary>
        /// 上一轮这一行是否已经等于触发值。按行号记住，下一次启动会换新实例。
        /// </summary>
        private readonly Dictionary<int, bool> _matched = [];

        /// <summary>
        /// 解析报警配置，日志写到流程日志。
        /// </summary>
        public LuxAlarmLogFlowTool(string cfg)
            : this(cfg, LogHelper.FlowInfo, LogAlarmError)
        {
        }

        /// <summary>
        /// 解析报警配置。测试传入自己的日志委托，避免写盘。
        /// </summary>
        internal LuxAlarmLogFlowTool(string cfg, Action<string> info, Action<string> error)
        {
            _info = info;
            _errorLog = error;
            if (!TryRead<LuxAlarmLogToolEntity>(cfg, out var tool, out var readError))
            {
                _error = readError;
                return;
            }

            _tool = tool;
        }

        /// <summary>
        /// 读每一行。空白地址跳过。一行失败记日志后继续。整步仍成功。
        /// </summary>
        public override FlowStepResult Execute(FlowStepContext context)
        {
            if (_error is not null || _tool is null)
                return Fail(_error ?? "配置无效");

            var rows = _tool.Rows ?? [];
            for (var index = 0; index < rows.Count; index++)
            {
                if (context.Token.IsCancellationRequested)
                    return Cancelled();

                var row = rows[index];
                if (row is null || string.IsNullOrWhiteSpace(row.Address))
                    continue;

                var address = row.Address.Trim();
                var read = context.Devices.Read(address, row.DataType, 1, context.Token);
                var step = FromPlc(context, read);
                if (step.Status == FlowStepStatus.Cancelled)
                    return step;

                if (step.Status != FlowStepStatus.Ok)
                {
                    _errorLog(Line(address, step.Text));
                    continue;
                }

                var matched = Matches(row.DataType, step.Text, row.TriggerValue);
                var was = _matched.TryGetValue(index, out var previous) && previous;
                _matched[index] = matched;
                if (matched && !was && _tool.Log)
                    _info(Line(row.Description, address, step.Text));
            }

            return Ok("");
        }

        /// <summary>
        /// FlowError 带可选异常，不能直接当成 Action。
        /// </summary>
        private static void LogAlarmError(string message)
        {
            LogHelper.FlowError(message);
        }

        /// <summary>
        /// 数值按类型解析后比较。解析不了时比较去掉首尾空白的原文。
        /// </summary>
        private static bool Matches(PlcDataType dataType, string actual, string expect)
        {
            var left = (actual ?? "").Trim();
            var right = (expect ?? "").Trim();
            switch (dataType)
            {
                case PlcDataType.Int16:
                    if (short.TryParse(left, NumberStyles.Integer, CultureInfo.InvariantCulture, out var left16)
                        && short.TryParse(right, NumberStyles.Integer, CultureInfo.InvariantCulture, out var right16))
                        return left16 == right16;
                    break;
                case PlcDataType.UInt16:
                    if (ushort.TryParse(left, NumberStyles.Integer, CultureInfo.InvariantCulture, out var leftU16)
                        && ushort.TryParse(right, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rightU16))
                        return leftU16 == rightU16;
                    break;
                case PlcDataType.Int32:
                    if (int.TryParse(left, NumberStyles.Integer, CultureInfo.InvariantCulture, out var left32)
                        && int.TryParse(right, NumberStyles.Integer, CultureInfo.InvariantCulture, out var right32))
                        return left32 == right32;
                    break;
                case PlcDataType.Float:
                    if (TryFloat(left, out var leftReal) && TryFloat(right, out var rightReal))
                        return leftReal == rightReal;
                    break;
            }

            return string.Equals(left, right, StringComparison.Ordinal);
        }

        /// <summary>
        /// 先按不变文化解析浮点，再按当前文化。与设备库的写入规则一致。
        /// </summary>
        private static bool TryFloat(string text, out float value)
        {
            return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                || float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
        }

        /// <summary>
        /// 把非空片段拼成一行，中间一个空格。
        /// </summary>
        private static string Line(params string[] parts)
        {
            return string.Join(" ", parts.Select(item => (item ?? "").Trim()).Where(item => item.Length > 0));
        }
    }
}
