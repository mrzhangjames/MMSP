using System.Runtime.CompilerServices;
using MMSP.Core.Model.Base;
using Prism.Mvvm;

namespace MMSP.Plugin.Tool.ViewModels
{
    /// <summary>
    /// 工具配置界面的公共编辑模型。名称写回工具。
    /// </summary>
    public abstract class ToolEditViewModel<TTool> : BindableBase
        where TTool : PluginToolModel
    {
        /// <summary>
        /// 创建编辑模型。
        /// </summary>
        protected ToolEditViewModel(TTool tool)
        {
            ArgumentNullException.ThrowIfNull(tool);
            Tool = tool;
        }

        /// <summary>
        /// 正在编辑的工具。
        /// </summary>
        public TTool Tool { get; }

        /// <summary>
        /// 正在编辑的工具。窗口按这个读回配置。
        /// </summary>
        public PluginToolModel EditedTool => Tool;

        /// <summary>
        /// 工具名称。
        /// </summary>
        public string Name
        {
            get => Tool.Name;
            set => SetText(Tool.Name, value, text => Tool.Name = text);
        }

        /// <summary>
        /// 把字符串写回工具。空引用按空字符串保存。
        /// </summary>
        protected void SetText(string current, string? value, Action<string> write, [CallerMemberName] string? propertyName = null)
        {
            var next = value ?? "";
            if (current == next)
                return;

            write(next);
            RaisePropertyChanged(propertyName);
        }

        /// <summary>
        /// 把整数写回工具。
        /// </summary>
        protected void SetNumber(int current, int value, Action<int> write, [CallerMemberName] string? propertyName = null)
        {
            if (current == value)
                return;

            write(value);
            RaisePropertyChanged(propertyName);
        }

        /// <summary>
        /// 把一个值写回工具。
        /// </summary>
        protected void SetValue<TValue>(TValue current, TValue value, Action<TValue> write, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<TValue>.Default.Equals(current, value))
                return;

            write(value);
            RaisePropertyChanged(propertyName);
        }
    }
}
