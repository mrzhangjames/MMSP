namespace MMSP.App.WpfBase.Services
{
    /// <summary>
    /// 客户插件提供的工具界面。流程页按配置装入，保存前先提交编辑。
    /// </summary>
    public interface ICustomerToolView
    {
        /// <summary>
        /// 按步骤里已有的配置填充界面。
        /// </summary>
        void Load(string cfg);

        /// <summary>
        /// 把正在编辑的内容写回配置。
        /// </summary>
        void CommitEdit();
    }
}
