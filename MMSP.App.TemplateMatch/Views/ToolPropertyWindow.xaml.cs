using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MMSP.App.WpfBase.Services;
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
        /// <summary>
        /// 正在编辑的工具种类。
        /// </summary>
        private readonly FlowToolKind _kind;

        /// <summary>
        /// 工具名称为空时用的标题。
        /// </summary>
        private readonly string _fallbackTitle;

        /// <summary>
        /// 当前窗口能否保存。当前客户没有这个工具时为 false。
        /// </summary>
        private readonly bool _editable;

        /// <summary>
        /// 打开一种工具的配置。fallbackTitle 是种类名称，工具名称为空时用它。
        /// 客户工具从已登记的插件取界面。当前客户没有这个工具时只提示，不能保存。
        /// </summary>
        public ToolPropertyWindow(FlowToolKind kind, string cfg, string fallbackTitle, CustomerPluginCatalog? catalog)
        {
            _kind = kind;
            _fallbackTitle = fallbackTitle;
            InitializeComponent();
            if (catalog?.Create(kind, cfg) is UserControl customer)
            {
                if (kind == FlowToolKind.Lux_AlarmLogTool)
                    UseAlarmSize();

                Body.Content = customer;
                _editable = true;
                return;
            }

            if (IsGeneral(kind))
            {
                var scanners = kind is FlowToolKind.SingleScan or FlowToolKind.MultiScan ? ReadScanners() : [];
                Body.Content = ToolCatalog.Create(kind, cfg, scanners);
                _editable = true;
                return;
            }

            _editable = false;
            SaveButton.IsEnabled = false;
            Body.Content = new TextBlock
            {
                Text = UnavailableText(),
                Margin = new Thickness(20, 16, 20, 16),
                TextWrapping = TextWrapping.Wrap
            };
        }

        /// <summary>
        /// 通用工具是枚举里的 0 到 8。
        /// </summary>
        private static bool IsGeneral(FlowToolKind kind)
        {
            return (int)kind <= (int)FlowToolKind.Script;
        }

        /// <summary>
        /// 立讯报警表格需要可调整的大窗口。
        /// </summary>
        private void UseAlarmSize()
        {
            Width = 760;
            MinWidth = 640;
            Height = 520;
            MinHeight = 360;
            MaxHeight = 800;
            SizeToContent = SizeToContent.Manual;
            ResizeMode = ResizeMode.CanResize;
        }

        /// <summary>
        /// 当前客户没有这个工具时的提示。
        /// </summary>
        private static string UnavailableText()
        {
            if (Application.Current?.TryFindResource("Flow_ToolUnavailable") is string value && value.Length > 0)
                return value;

            return "当前客户未加载此工具";
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
            if (!_editable)
                return;

            if (Body.Content is ICustomerToolView customer)
                customer.CommitEdit();

            if (Body.Content is not UserControl view)
                return;

            var cfg = ToolCatalog.Config(view);
            Result = new ToolEditResult
            {
                Title = Limit(ToolCatalog.Title(_kind, cfg, _fallbackTitle)),
                Detail = ToolCatalog.Detail(_kind, cfg),
                CfgValue = cfg
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
    }
}
