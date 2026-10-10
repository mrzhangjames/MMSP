using System.ComponentModel;

namespace MMSP.Core.Model.Sys
{
    /// <summary>
    /// 导航右侧的运行状态。底部状态文字与这里的说明一致。
    /// </summary>
    public enum RunState : uint
    {
        /// <summary>
        /// 启动
        /// </summary>
        [Description("启动")]
        Start = 0,

        /// <summary>
        /// 停止
        /// </summary>
        [Description("停止")]
        Stop = 1,

        /// <summary>
        /// 刷新
        /// </summary>
        [Description("刷新")]
        Refresh = 2
    }
}
