using System.Text.Json;
using MMSP.Core.Model.Base;
using MMSP.Core.Model.Tool;
using MMSP.Service.Device.Plc;
using MMSP.Service.Device.Scan;

namespace MMSP.Service.Flow.Steps
{
    /// <summary>
    /// 一种流程工具。子类在 <see cref="Execute"/> 里完成自己的调用。
    /// </summary>
    public abstract class FlowToolStep
    {
        /// <summary>
        /// 与属性窗口相同的 Json 选项。属性名忽略大小写，不改命名。
        /// </summary>
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// 执行这一步。预期内的失败返回结果，不向外抛。
        /// </summary>
        public abstract FlowStepResult Execute(FlowStepContext context);

        /// <summary>
        /// 成功。
        /// </summary>
        protected static FlowStepResult Ok(string text)
        {
            return FlowStepResult.Ok(text);
        }

        /// <summary>
        /// 失败。
        /// </summary>
        protected static FlowStepResult Fail(string reason)
        {
            return FlowStepResult.Fail(reason);
        }

        /// <summary>
        /// 已取消。
        /// </summary>
        protected static FlowStepResult Cancelled()
        {
            return FlowStepResult.Cancelled();
        }

        /// <summary>
        /// 读出工具配置。空白或损坏时失败，不换成空配置继续跑。
        /// </summary>
        protected static bool TryRead<TTool>(string cfg, out TTool tool, out string error)
            where TTool : PluginToolModel, new()
        {
            tool = new TTool();
            if (string.IsNullOrWhiteSpace(cfg))
            {
                error = "配置无效";
                return false;
            }

            try
            {
                var read = JsonSerializer.Deserialize<TTool>(cfg, Json);
                if (read is null)
                {
                    error = "配置无效";
                    return false;
                }

                tool = read;
                error = "";
                return true;
            }
            catch (JsonException)
            {
                error = "配置无效";
                return false;
            }
        }

        /// <summary>
        /// 把 PLC 结果收成一步结果。已经取消时不再当成通讯失败。
        /// </summary>
        protected static FlowStepResult FromPlc(FlowStepContext context, PlcIoResult result)
        {
            if (context.Token.IsCancellationRequested)
                return Cancelled();

            if (result.IsSuccess)
                return Ok(result.Value);

            return Fail(string.IsNullOrWhiteSpace(result.Message) ? "通讯失败" : result.Message);
        }

        /// <summary>
        /// 成功时把结果文本写入出参。名称为空则不写。名称不在名单里时这一步失败，并且不写。
        /// </summary>
        protected static FlowStepResult Store(FlowStepContext context, string? name, FlowStepResult result)
        {
            if (result.Status != FlowStepStatus.Ok)
                return result;

            if (string.IsNullOrWhiteSpace(name))
                return result;

            var key = name.Trim();
            if (!context.Variables.Contains(key))
                return Fail("变量未声明");

            context.Variables.Set(key, result.Text);
            return result;
        }

        /// <summary>
        /// 入参里填了的变量必须已经声明。没有入参时返回空。
        /// </summary>
        protected static FlowStepResult? RequireInputs(FlowStepContext context, IEnumerable<ToolInput>? inputs)
        {
            if (inputs is null)
                return null;

            foreach (var input in inputs)
            {
                var name = (input?.Variable ?? "").Trim();
                if (name.Length == 0)
                    continue;

                if (!context.Variables.Contains(name))
                    return Fail("变量未声明");
            }

            return null;
        }

        /// <summary>
        /// 把扫码结果收成一步结果。已经取消时不再当成通讯失败。
        /// </summary>
        protected static FlowStepResult FromScan(FlowStepContext context, ScanIoResult result)
        {
            if (context.Token.IsCancellationRequested)
                return Cancelled();

            if (result.IsSuccess)
                return Ok(result.Value);

            return Fail(string.IsNullOrWhiteSpace(result.Message) ? "通讯失败" : result.Message);
        }
    }
}
