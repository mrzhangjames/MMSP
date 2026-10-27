using MMSP.Core.Model.Base;
using MMSP.Core.Model.Tool;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// CSV。按列读取流程变量，追加到 Data\File\流程名 下的文件。不写出参。
    /// </summary>
    public sealed class CsvFlowTool : FlowToolStep
    {
        /// <summary>
        /// CSV 配置。解析失败时为空。
        /// </summary>
        private readonly CsvToolEntity? _tool;

        /// <summary>
        /// 解析失败的原因。成功时为空。
        /// </summary>
        private readonly string? _error;

        /// <summary>
        /// 测试时改到临时目录。为空时用客户数据根。
        /// </summary>
        internal string? DataRoot { get; set; }

        /// <summary>
        /// 解析配置。文件在执行时按流程名和当天日期确定。
        /// </summary>
        public CsvFlowTool(string cfg)
        {
            if (!TryRead<CsvToolEntity>(cfg, out var tool, out var error))
            {
                _error = error;
                return;
            }

            _tool = tool;
        }

        /// <summary>
        /// 校验列和变量后追加一行。取消、配置无效或表头不一致时不写。
        /// </summary>
        public override FlowStepResult Execute(FlowStepContext context)
        {
            if (context.Token.IsCancellationRequested)
                return Cancelled();

            if (_error is not null || _tool is null)
                return Fail(_error ?? "配置无效");

            if (context.Token.IsCancellationRequested)
                return Cancelled();

            var error = CsvExport.Append(
                _tool,
                context.FlowName,
                context.Variables,
                DateTime.Now,
                string.IsNullOrWhiteSpace(DataRoot) ? PathCore.DataDirectory : DataRoot,
                out var written);
            if (error.Length > 0)
                return Fail(error);

            return Ok(written);
        }
    }
}
