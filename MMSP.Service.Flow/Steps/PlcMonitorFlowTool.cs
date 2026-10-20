using System.Globalization;
using MMSP.Core.Model.Sys;
using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// PLC 监控。读到期望值时结束，需要重置时再写回。
    /// </summary>
    public sealed class PlcMonitorFlowTool : FlowToolStep
    {
        /// <summary>
        /// 监控配置。解析失败时为空。
        /// </summary>
        private readonly PlcMonitorToolEntity? _tool;

        /// <summary>
        /// 解析失败的原因。成功时为空。
        /// </summary>
        private readonly string? _error;

        /// <summary>
        /// 解析监控配置。
        /// </summary>
        public PlcMonitorFlowTool(string cfg)
        {
            if (!TryRead<PlcMonitorToolEntity>(cfg, out var tool, out var error))
            {
                _error = error;
                return;
            }

            _tool = tool;
        }

        /// <summary>
        /// 按间隔读取，直到相等、失败或取消。
        /// </summary>
        public override FlowStepResult Execute(FlowStepContext context)
        {
            if (_error is not null || _tool is null)
                return Fail(_error ?? "配置无效");

            if (string.IsNullOrWhiteSpace(_tool.Address))
                return Fail("地址为空");

            if (_tool.Interval <= 0)
                return Fail("监控间隔无效");

            var address = _tool.Address.Trim();
            while (true)
            {
                if (context.Token.IsCancellationRequested)
                    return Cancelled();

                var read = context.Devices.Read(address, _tool.DataType, _tool.Length, context.Token);
                var step = FromPlc(context, read);
                if (step.Status != FlowStepStatus.Ok)
                    return step;

                if (!Matches(_tool.DataType, step.Text, _tool.ExpectValue))
                {
                    if (!context.Devices.Wait(_tool.Interval, context.Token))
                        return Cancelled();

                    continue;
                }

                if (!_tool.Reset)
                    return Ok(step.Text);

                var write = context.Devices.Write(address, _tool.DataType, _tool.ResetValue ?? "", context.Token);
                var written = FromPlc(context, write);
                if (written.Status != FlowStepStatus.Ok)
                    return written;

                return Ok(step.Text);
            }
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
    }
}
