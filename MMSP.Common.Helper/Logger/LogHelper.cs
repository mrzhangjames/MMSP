using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using MMSP.Core.License;
using MMSP.Core.Model.Base;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Helper;
using MMSP.Core.Model.Sys;
using MMSP.Service.Database;
using NLog;
using NLog.Config;
using Prism.Events;
using SqlSugar;

namespace MMSP.Common.Helper.Logger
{
    /// <summary>
    /// 全局日志。按 NLog.config 写入运行日志，需要时再抄一份专项文件，并写入客户库、通知主页。
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// 客户库里的来源名。文件记录器的名字在 NLog.config 里分开。
        /// </summary>
        private const string LoggerName = "MMSP";

        /// <summary>
        /// 运行日志。每条日志都写这一份。
        /// </summary>
        private const string RunLoggerName = "MMSP.Run";

        /// <summary>
        /// 设备日志。只收设备读写。
        /// </summary>
        private const string DeviceLoggerName = "MMSP.Device";

        /// <summary>
        /// 流程日志。只收流程运行。
        /// </summary>
        private const string FlowLoggerName = "MMSP.Flow";

        /// <summary>
        /// MES 日志。只收 MES 交互。
        /// </summary>
        private const string MesLoggerName = "MMSP.Mes";

        /// <summary>
        /// 只配置一次文件日志。
        /// </summary>
        private static readonly object Gate = new();

        /// <summary>
        /// 文件是否已经配置。
        /// </summary>
        private static bool _fileReady;

        /// <summary>
        /// 主程序交来的事件聚合器。没调用 <see cref="Use"/> 之前为空。
        /// </summary>
        private static IEventAggregator? _aggregator;

        /// <summary>
        /// 除运行日志外再写哪一份专项文件。
        /// </summary>
        private enum LogCopy
        {
            /// <summary>
            /// 只写运行日志。
            /// </summary>
            None = 0,

            /// <summary>
            /// 再写设备日志。
            /// </summary>
            Device = 1,

            /// <summary>
            /// 再写流程日志。
            /// </summary>
            Flow = 2,

            /// <summary>
            /// 再写 MES 日志。
            /// </summary>
            Mes = 3
        }

        /// <summary>
        /// 主页每个列表最多保留的条数。只限制本次进程的内存和界面，不删文件和客户库。
        /// </summary>
        public const int SessionRowLimit = 1000;

        /// <summary>
        /// 本次进程写出的日志，旧的在前。超过 <see cref="SessionRowLimit"/> 时丢掉最旧的。
        /// </summary>
        private static readonly List<RunLogMessage> SessionLogs = new();

        /// <summary>
        /// 接上 Prism 的事件聚合器。必须是容器里的那一个。
        /// </summary>
        public static void Use(IEventAggregator aggregator)
        {
            _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
            BindCaughtLog();
            EnsureFile();
        }

        /// <summary>
        /// 把下层库捕获到的异常接到本日志。不要求事件聚合器，启动读许可证之前就要接上。
        /// </summary>
        public static void BindCaughtLog()
        {
            CaughtLog.Error = (channel, message, exception) =>
            {
                switch (channel)
                {
                    case CaughtLogChannel.Plc:
                        PlcIoError(message, exception);
                        break;
                    case CaughtLogChannel.Scan:
                        ScanIoError(message, exception);
                        break;
                    case CaughtLogChannel.Flow:
                        FlowRunError(message, exception);
                        break;
                    case CaughtLogChannel.Mes:
                        MesIoError(message, exception);
                        break;
                    default:
                        SysError(message, exception);
                        break;
                }
            };
            LicenseCaughtLog.Error = (message, exception) => SysError(message, exception);
        }

        /// <summary>
        /// 写一条系统正常日志。
        /// </summary>
        public static void SysInfo(string message)
        {
            Write(RunLogBiz.Sys, RunLogLevel.Info, message, null);
        }

        /// <summary>
        /// 写一条系统报错日志。
        /// </summary>
        public static void SysError(string message, Exception? exception = null)
        {
            Write(RunLogBiz.Sys, RunLogLevel.Error, message, exception);
        }

        /// <summary>
        /// 写一条 PLC 正常日志。
        /// </summary>
        public static void PlcInfo(string message)
        {
            Write(RunLogBiz.Plc, RunLogLevel.Info, message, null);
        }

        /// <summary>
        /// 写一条 PLC 报错日志。
        /// </summary>
        public static void PlcError(string message, Exception? exception = null)
        {
            Write(RunLogBiz.Plc, RunLogLevel.Error, message, exception);
        }

        /// <summary>
        /// 写一条扫码正常日志。
        /// </summary>
        public static void ScanInfo(string message)
        {
            Write(RunLogBiz.Scan, RunLogLevel.Info, message, null);
        }

        /// <summary>
        /// 写一条扫码报错日志。
        /// </summary>
        public static void ScanError(string message, Exception? exception = null)
        {
            Write(RunLogBiz.Scan, RunLogLevel.Error, message, exception);
        }

        /// <summary>
        /// 写一条流程正常日志。
        /// </summary>
        public static void FlowInfo(string message)
        {
            Write(RunLogBiz.Flow, RunLogLevel.Info, message, null);
        }

        /// <summary>
        /// 写一条流程报错日志。
        /// </summary>
        public static void FlowError(string message, Exception? exception = null)
        {
            Write(RunLogBiz.Flow, RunLogLevel.Error, message, exception);
        }

        /// <summary>
        /// 写一条 MES 正常日志。
        /// </summary>
        public static void MesInfo(string message)
        {
            Write(RunLogBiz.Mes, RunLogLevel.Info, message, null);
        }

        /// <summary>
        /// 写一条 MES 报错日志。
        /// </summary>
        public static void MesError(string message, Exception? exception = null)
        {
            Write(RunLogBiz.Mes, RunLogLevel.Error, message, exception);
        }

        /// <summary>
        /// 写一条 PLC 读写日志。运行日志和设备日志各一份。
        /// </summary>
        public static void PlcIoInfo(string message)
        {
            Write(RunLogBiz.Plc, RunLogLevel.Info, message, null, LogCopy.Device);
        }

        /// <summary>
        /// 写一条 PLC 读写失败日志。运行日志和设备日志各一份。
        /// </summary>
        public static void PlcIoError(string message, Exception? exception = null)
        {
            Write(RunLogBiz.Plc, RunLogLevel.Error, message, exception, LogCopy.Device);
        }

        /// <summary>
        /// 写一条扫码读写日志。运行日志和设备日志各一份。
        /// </summary>
        public static void ScanIoInfo(string message)
        {
            Write(RunLogBiz.Scan, RunLogLevel.Info, message, null, LogCopy.Device);
        }

        /// <summary>
        /// 写一条扫码读写失败日志。运行日志和设备日志各一份。
        /// </summary>
        public static void ScanIoError(string message, Exception? exception = null)
        {
            Write(RunLogBiz.Scan, RunLogLevel.Error, message, exception, LogCopy.Device);
        }

        /// <summary>
        /// 写一条流程运行日志。运行日志和流程日志各一份。
        /// </summary>
        public static void FlowRunInfo(string message)
        {
            Write(RunLogBiz.Flow, RunLogLevel.Info, message, null, LogCopy.Flow);
        }

        /// <summary>
        /// 写一条流程运行失败日志。运行日志和流程日志各一份。
        /// </summary>
        public static void FlowRunError(string message, Exception? exception = null)
        {
            Write(RunLogBiz.Flow, RunLogLevel.Error, message, exception, LogCopy.Flow);
        }

        /// <summary>
        /// 写一条 MES 交互失败日志。运行日志和 MES 日志各一份。
        /// </summary>
        public static void MesIoError(string message, Exception? exception = null)
        {
            Write(RunLogBiz.Mes, RunLogLevel.Error, message, exception, LogCopy.Mes);
        }

        /// <summary>
        /// 读取本次进程写出的日志，旧的在前。不包括本次启动之前的历史。
        /// </summary>
        public static IReadOnlyList<RunLogMessage> LoadSession()
        {
            lock (Gate)
            {
                return SessionLogs.ToArray();
            }
        }

        /// <summary>
        /// 拼文件里的一行。线程名空白时只留线程号。客户库和主页不用这一行。
        /// </summary>
        internal static string FormatLine(
            DateTime time,
            int threadId,
            string? threadName,
            RunLogBiz biz,
            string levelText,
            string message)
        {
            var id = threadId.ToString(CultureInfo.InvariantCulture);
            var thread = string.IsNullOrWhiteSpace(threadName)
                ? "线程 " + id
                : "线程 " + id + " " + threadName.Trim();
            return $"{time:yyyy-MM-dd HH:mm:ss} [{thread}] [{biz}] [{levelText}] {message}";
        }

        /// <summary>
        /// 写文件、写客户库，然后发布消息。任一步失败都不抛给调用方。
        /// </summary>
        private static void Write(RunLogBiz biz, RunLogLevel level, string message, Exception? exception, LogCopy copy = LogCopy.None)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            message = message.Trim().Replace("\r", " ").Replace("\n", " ");
            var time = DateTime.Now;
            var levelText = ReadLevelText(level);
            var entity = new RunLogEntity
            {
                Id = Guid.NewGuid(),
                LogTime = time,
                Level = level,
                BizType = biz,
                Message = message,
                Exception = exception?.ToString(),
                Logger = LoggerName
            };

            try
            {
                var thread = Thread.CurrentThread;
                var line = FormatLine(time, thread.ManagedThreadId, thread.Name, biz, levelText, message);
                EnsureFile();
                var nlogLevel = ToNLogLevel(level);
                LogFile(RunLoggerName, nlogLevel, line);
                if (LoggerNameFor(copy) is string extra)
                    LogFile(extra, nlogLevel, line);
            }
            catch (Exception)
            {
                // 文件写失败时仍尝试入库和通知主页。
            }

            try
            {
                if (SqlSugarContext.IsOpen)
                    SqlSugarContext.CustomerDb.Insertable(entity).ExecuteCommand();
            }
            catch (Exception)
            {
                // 入库失败不影响调用方，文件里仍可能有这一条。
            }

            var row = ToMessage(entity);
            Remember(row);
            _aggregator?.GetEvent<RunLogEvent>().Publish(row);
        }

        /// <summary>
        /// 记下本次进程的一条日志，并丢掉超出界面上限的最旧记录。
        /// </summary>
        private static void Remember(RunLogMessage message)
        {
            lock (Gate)
            {
                SessionLogs.Add(message);
                var extra = SessionLogs.Count - SessionRowLimit;
                if (extra > 0)
                    SessionLogs.RemoveRange(0, extra);
            }
        }

        /// <summary>
        /// 读取程序目录里的 NLog.config。日志目录由 PathCore 赋给 logDir。
        /// </summary>
        private static void EnsureFile()
        {
            if (_fileReady)
                return;

            lock (Gate)
            {
                if (_fileReady)
                    return;

                Directory.CreateDirectory(PathCore.LogDirectory);
                LogManager.Setup().LoadConfigurationFromFile(PathCore.LogFilePath);
                LogManager.Configuration.Variables["logDir"] = PathCore.LogDirectory;
                _fileReady = true;
            }
        }

        /// <summary>
        /// 按记录器名字写一行。文件夹由 NLog.config 决定。
        /// </summary>
        private static void LogFile(string loggerName, NLog.LogLevel level, string line)
        {
            var evt = new LogEventInfo(level, loggerName, CultureInfo.InvariantCulture, "{0}", new object[] { line });
            LogManager.GetLogger(loggerName).Log(evt);
        }

        /// <summary>
        /// 专项文件使用的记录器。只写运行日志时返回空。
        /// </summary>
        private static string? LoggerNameFor(LogCopy copy)
        {
            return copy switch
            {
                LogCopy.Device => DeviceLoggerName,
                LogCopy.Flow => FlowLoggerName,
                LogCopy.Mes => MesLoggerName,
                _ => null
            };
        }

        /// <summary>
        /// 转成主页用的消息。
        /// </summary>
        private static RunLogMessage ToMessage(RunLogEntity entity)
        {
            return new RunLogMessage
            {
                Id = entity.Id,
                LogTime = entity.LogTime,
                Level = entity.Level,
                LevelText = ReadLevelText(entity.Level),
                BizType = entity.BizType,
                Message = entity.Message
            };
        }

        /// <summary>
        /// 读取级别上的中文说明。
        /// </summary>
        private static string ReadLevelText(RunLogLevel level)
        {
            var field = typeof(RunLogLevel).GetField(level.ToString());
            var description = field?.GetCustomAttribute<DescriptionAttribute>();
            return description?.Description ?? level.ToString();
        }

        /// <summary>
        /// 对应到 NLog 级别，只用于文件规则过滤。
        /// </summary>
        private static NLog.LogLevel ToNLogLevel(RunLogLevel level)
        {
            return level switch
            {
                RunLogLevel.Alarm => NLog.LogLevel.Warn,
                RunLogLevel.Error => NLog.LogLevel.Error,
                _ => NLog.LogLevel.Info
            };
        }
    }
}
