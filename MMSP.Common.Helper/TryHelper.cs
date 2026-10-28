using System.ComponentModel;
using System.Security;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.Sys;

namespace MMSP.Common.Helper
{
    /// <summary>
    /// 执行一段代码。只捕获已经分出的异常类型并写入运行日志，其它异常继续抛出。
    /// 操作取消不记异常。业务代码按自己实际会抛出的类型捕获，不把整段逻辑交给这里。
    /// </summary>
    public static class TryHelper
    {
        /// <summary>
        /// 执行动作。成功返回 true。已分出的异常写入日志并返回 false。
        /// </summary>
        public static bool Run(string? action, Action work, RunLogBiz biz = RunLogBiz.Sys)
        {
            if (work is null)
                throw new ArgumentNullException(nameof(work));

            return Guard(action, work, biz);
        }

        /// <summary>
        /// 执行并取出结果。失败时 result 为默认值，并写入日志。
        /// </summary>
        public static bool Run<T>(string? action, Func<T> work, out T? result, RunLogBiz biz = RunLogBiz.Sys)
        {
            if (work is null)
                throw new ArgumentNullException(nameof(work));

            T? value = default;
            var ok = Guard(action, () => { value = work(); }, biz);
            result = ok ? value : default;
            return ok;
        }

        /// <summary>
        /// 执行异步动作。成功返回 true。已分出的异常写入日志并返回 false。
        /// </summary>
        public static Task<bool> RunAsync(string? action, Func<Task> work, RunLogBiz biz = RunLogBiz.Sys)
        {
            if (work is null)
                throw new ArgumentNullException(nameof(work));

            return GuardAsync(action, work, biz);
        }

        /// <summary>
        /// 执行异步动作并取出结果。失败时返回默认值，并写入日志。
        /// </summary>
        public static async Task<(bool Ok, T? Value)> RunAsync<T>(string? action, Func<Task<T>> work, RunLogBiz biz = RunLogBiz.Sys)
        {
            if (work is null)
                throw new ArgumentNullException(nameof(work));

            T? value = default;
            var ok = await GuardAsync(action, async () => { value = await work().ConfigureAwait(false); }, biz)
                .ConfigureAwait(false);
            return (ok, ok ? value : default);
        }

        /// <summary>
        /// 按实际类型捕获。派生类写在基类前面。没有列出的异常继续抛出。
        /// </summary>
        private static bool Guard(string? action, Action work, RunLogBiz biz)
        {
            try
            {
                work();
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (DecoderFallbackException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (ArgumentNullException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (ArgumentException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (SecurityException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (IOException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (ObjectDisposedException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (InvalidOperationException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (JsonException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (FormatException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (CryptographicException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (Win32Exception ex)
            {
                return Handle(action, biz, ex);
            }
            catch (TimeoutException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (NotSupportedException ex)
            {
                return Handle(action, biz, ex);
            }
        }

        /// <summary>
        /// 异步版本。捕获的类型和 <see cref="Guard"/> 相同。
        /// </summary>
        private static async Task<bool> GuardAsync(string? action, Func<Task> work, RunLogBiz biz)
        {
            try
            {
                await work().ConfigureAwait(false);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (DecoderFallbackException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (ArgumentNullException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (ArgumentException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (SecurityException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (IOException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (ObjectDisposedException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (InvalidOperationException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (JsonException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (FormatException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (CryptographicException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (Win32Exception ex)
            {
                return Handle(action, biz, ex);
            }
            catch (TimeoutException ex)
            {
                return Handle(action, biz, ex);
            }
            catch (NotSupportedException ex)
            {
                return Handle(action, biz, ex);
            }
        }

        /// <summary>
        /// 这种异常在日志里使用的短说明。不包含异常原文。
        /// </summary>
        internal static string Describe(Exception exception)
        {
            return exception switch
            {
                DecoderFallbackException => "文本编码无效",
                ArgumentNullException => "参数为空",
                ArgumentException => "参数无效",
                UnauthorizedAccessException => "没有访问权限",
                SecurityException => "安全检查未通过",
                IOException => "文件读写失败",
                ObjectDisposedException => "对象已释放",
                InvalidOperationException => "当前状态不能执行",
                JsonException => "JSON 无法解析",
                FormatException => "内容格式无效",
                CryptographicException => "校验失败",
                Win32Exception => "系统调用失败",
                TimeoutException => "操作超时",
                NotSupportedException => "不支持该操作",
                _ => "操作失败"
            };
        }

        /// <summary>
        /// 拼成写入日志的一句。动作为空时只留类型说明。
        /// </summary>
        internal static string FormatMessage(string? action, Exception exception)
        {
            var reason = Describe(exception);
            if (string.IsNullOrWhiteSpace(action))
                return reason;

            return action.Trim() + "：" + reason;
        }

        /// <summary>
        /// 取消不算异常。其余类型记一条错误日志。
        /// </summary>
        private static bool Handle(string? action, RunLogBiz biz, Exception exception)
        {
            if (exception is OperationCanceledException)
                return false;

            var message = FormatMessage(action, exception);
            switch (biz)
            {
                case RunLogBiz.Plc:
                    LogHelper.PlcError(message, exception);
                    break;
                case RunLogBiz.Scan:
                    LogHelper.ScanError(message, exception);
                    break;
                case RunLogBiz.Flow:
                    LogHelper.FlowError(message, exception);
                    break;
                case RunLogBiz.Mes:
                    LogHelper.MesError(message, exception);
                    break;
                default:
                    LogHelper.SysError(message, exception);
                    break;
            }

            return false;
        }
    }
}
