using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using MMSP.Core.Model.Base;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Service.Database;
using NLog;
using NLog.Config;
using Prism.Events;
using SqlSugar;

namespace MMSP.Common.Helper.Logger
{
    /// <summary>
    /// 全局日志。写入当天文件和客户库，再通知主页。
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// 文件和库里的来源名。
        /// </summary>
        private const string LoggerName = "MMSP";

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
        /// 写入文件的 NLog 记录器。
        /// </summary>
        private static NLog.Logger? _fileLogger;

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
            EnsureFile();
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
        /// 写文件、写客户库，然后发布消息。任一步失败都不抛给调用方。
        /// </summary>
        private static void Write(RunLogBiz biz, RunLogLevel level, string message, Exception? exception)
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
                EnsureFile();
                var line = $"{time:yyyy-MM-dd HH:mm:ss} [{biz}] [{levelText}] {message}";
                var nlogLevel = ToNLogLevel(level);
                var evt = new LogEventInfo(nlogLevel, LoggerName, CultureInfo.InvariantCulture, "{0}", new object[] { line });
                _fileLogger?.Log(evt);
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
                _fileLogger = LogManager.GetLogger(LoggerName);
                _fileReady = true;
            }
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
