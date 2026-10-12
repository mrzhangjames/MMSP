using Prism.Events;

namespace MMSP.Common.Helper.Logger
{
    /// <summary>
    /// 新日志消息。只由 <see cref="LogHelper"/> 发布，主页订阅。
    /// </summary>
    public sealed class RunLogEvent : PubSubEvent<RunLogMessage>
    {
    }
}
