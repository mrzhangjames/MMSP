using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Plugin.Tool;
using MMSP.Service.Database;

namespace MMSP.App.TemplateMatch.Views
{
    /// <summary>
    /// 流程步的工具属性窗口。使用 Metro 窗口，确定后把配置交回流程页。
    /// </summary>
    public partial class ToolPropertyWindow : MetroWindow
    {
        private readonly FlowToolKind _kind;
        private readonly string _fallbackTitle;

        /// <summary>
        /// 打开一种工具的配置。fallbackTitle 是种类名称，工具名称为空时用它。
        /// </summary>
        public ToolPropertyWindow(FlowToolKind kind, string cfg, string fallbackTitle)
        {
            _kind = kind;
            _fallbackTitle = fallbackTitle;
            InitializeComponent();
            var scanners = kind is FlowToolKind.SingleScan or FlowToolKind.MultiScan ? ReadScanners() : [];
            Body.Content = ToolCatalog.Create(kind, cfg, scanners);
        }

        /// <summary>
        /// 确定后的配置。取消时为空。
        /// </summary>
        public ToolEditResult? Result { get; private set; }

        /// <summary>
        /// 保存窗口里的工具配置并关闭。
        /// </summary>
        private void Accept(object sender, RoutedEventArgs e)
        {
            if (Body.Content is not UserControl view)
                return;

            var cfg = ToolCatalog.Config(view);
            Result = new ToolEditResult
            {
                Title = Limit(ToolCatalog.Title(_kind, cfg, _fallbackTitle)),
                Detail = ToolCatalog.Detail(_kind, cfg),
                CfgValue = cfg,
                NumberValue = ToolCatalog.NumberValue(_kind, cfg)
            };
            DialogResult = true;
        }

        /// <summary>
        /// 按排序读取扫码枪名称。库没打开或读取失败时返回空列表，窗口仍可编辑。
        /// </summary>
        private static IReadOnlyList<string> ReadScanners()
        {
            if (!SqlSugarContext.IsOpen)
                return [];

            try
            {
                return SqlSugarContext.CustomerDb.Queryable<ScannerEntity>()
                    .OrderBy(item => item.SortIndex)
                    .ToList()
                    .Select(item => item.Name.Trim())
                    .Where(item => item.Length > 0)
                    .Distinct()
                    .ToList();
            }
            catch (Exception ex)
            {
                LogHelper.ScanError("读取扫码枪失败", ex);
                return [];
            }
        }

        /// <summary>
        /// 标题最长 32，和流程步表一致。
        /// </summary>
        private static string Limit(string text)
        {
            var value = text.Trim();
            return value.Length <= 32 ? value : value[..32];
        }
    }

    /// <summary>
    /// 属性窗口确认后写回流程步的内容。
    /// </summary>
    public sealed class ToolEditResult
    {
        /// <summary>
        /// 步骤标题。
        /// </summary>
        public string Title { get; init; } = "";

        /// <summary>
        /// 步骤说明。
        /// </summary>
        public string Detail { get; init; } = "";

        /// <summary>
        /// 工具配置。
        /// </summary>
        public string CfgValue { get; init; } = "";

        /// <summary>
        /// 延时毫秒。
        /// </summary>
        public int NumberValue { get; init; }
    }
}
