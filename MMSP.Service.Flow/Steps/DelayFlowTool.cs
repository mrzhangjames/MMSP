using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// 延时。在循环线程上等待，停止可以取消。
    /// </summary>
    public sealed class DelayFlowTool : FlowToolStep
    {
        /// <summary>
        /// 延时配置。解析失败时为空。
        /// </summary>
        private readonly DelayToolEntity? _tool;

        /// <summary>
        /// 解析失败的原因。成功时为空。
        /// </summary>
        private readonly string? _error;

        /// <summary>
        /// 解析延时配置。
        /// </summary>
        public DelayFlowTool(string cfg)
        {
            if (!TryRead<DelayToolEntity>(cfg, out var tool, out var error))
            {
                _error = error;
                return;
            }

            _tool = tool;
        }

        /// <summary>
        /// 等待配置的毫秒数。小于 0 视为配置无效。
        /// </summary>
        public override FlowStepResult Execute(FlowStepContext context)
        {
            if (_error is not null || _tool is null)
                return Fail(_error ?? "配置无效");

            if (_tool.Milliseconds < 0)
                return Fail("配置无效");

            if (context.Token.IsCancellationRequested)
                return Cancelled();

            try
            {
                // 循环在后台线程上，这里等待可以被停止取消。
                Task.Delay(_tool.Milliseconds, context.Token).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                return Cancelled();
            }

            return Ok("");
        }
    }
}
