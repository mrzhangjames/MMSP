using System.Windows;
using GongSolutions.Wpf.DragDrop;
using MMSP.App.TemplateMatch.Models;
using MMSP.App.TemplateMatch.Views;
using MMSP.App.WpfBase.Services;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Tool;
using MMSP.Service.Database;
using SqlSugar;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 流程页的工具拖放和流程步属性。
    /// </summary>
    public partial class FlowViewModel
    {
        /// <summary>
        /// 工具拖入时复制新增。流程步在本列表内拖动时移动顺序。没有选中流程时不接收。
        /// </summary>
        public void DragOver(IDropInfo dropInfo)
        {
            if (LicenseGate.IsExpired(ContainerExt) || SelectedFlow is null)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            if (DraggedTool(dropInfo) is not null)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Copy;
                return;
            }

            if (DraggedStep(dropInfo) is FlowStep step && SelectedFlow.Steps.Contains(step))
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// 放下工具时新增流程步。放下流程步时按插入位置调整顺序。
        /// </summary>
        public void Drop(IDropInfo dropInfo)
        {
            if (LicenseGate.Block(ContainerExt) || SelectedFlow is null)
                return;

            if (DraggedTool(dropInfo) is FlowTool tool)
            {
                var index = dropInfo.InsertIndex;
                if (index < 0 || index > SelectedFlow.Steps.Count)
                    index = SelectedFlow.Steps.Count;

                InsertStep(SelectedFlow, tool, index);
                return;
            }

            if (DraggedStep(dropInfo) is FlowStep step)
                MoveStep(SelectedFlow, step, dropInfo.InsertIndex);
        }

        /// <summary>
        /// 取出正在拖的工具。控件库带回的数据包可能是空的，这时用拖放源上的原对象。
        /// </summary>
        private static FlowTool? DraggedTool(IDropInfo dropInfo)
        {
            if (dropInfo.Data is FlowTool tool)
                return tool;

            if (dropInfo.DragInfo?.Data is FlowTool dragged)
                return dragged;

            return dropInfo.DragInfo?.SourceItem as FlowTool;
        }

        /// <summary>
        /// 取出正在拖的流程步。控件库带回的数据包可能是空的，这时用拖放源上的原对象。
        /// </summary>
        private static FlowStep? DraggedStep(IDropInfo dropInfo)
        {
            if (dropInfo.Data is FlowStep step)
                return step;

            if (dropInfo.DragInfo?.Data is FlowStep dragged)
                return dragged;

            return dropInfo.DragInfo?.SourceItem as FlowStep;
        }

        /// <summary>
        /// 选中一张流程步。其它步骤取消选中。
        /// </summary>
        private void SelectStep(FlowStep? step)
        {
            if (SelectedFlow is null || step is null)
                return;

            foreach (var item in SelectedFlow.Steps)
                item.IsSelected = ReferenceEquals(item, step);
        }

        /// <summary>
        /// 打开流程步的工具属性。确定后写回配置。
        /// </summary>
        private async void ShowProperty(FlowStep? step)
        {
            if (LicenseGate.Block(ContainerExt) || step is null)
                return;

            SelectStep(step);
            CustomerPluginCatalog? catalog = IContainerProviderExtensions.IsRegistered<CustomerPluginCatalog>(ContainerExt)
                ? ContainerExt.Resolve<CustomerPluginCatalog>()
                : null;
            var window = new ToolPropertyWindow(
                step.ToolKind,
                step.CfgValue,
                step.Kind,
                catalog,
                VariableNames(SelectedFlow),
                PriorOutputs(SelectedFlow, step),
                OtherOutputs(SelectedFlow, step),
                SelectedFlow?.Name ?? "");
            var owner = Application.Current?.MainWindow;
            if (owner is not null && owner.IsLoaded)
                window.Owner = owner;

            if (window.ShowDialog() != true || window.Result is null)
                return;

            if (!SaveProperty(step, window.Result))
                await MetroDialogService.ShowMessageAsync(Text("Flow_Property", "属性"), Text("User_SaveFailed", "保存失败"));
        }

        /// <summary>
        /// 本流程已保存的变量名，按显示顺序。
        /// </summary>
        private static IReadOnlyList<string> VariableNames(FlowCard? flow)
        {
            if (flow is null || !SqlSugarContext.IsOpen)
                return [];

            try
            {
                return SqlSugarContext.CustomerDb.Queryable<FlowVarEntity>()
                    .Where(item => item.FlowId == flow.Id)
                    .OrderBy(item => item.SortIndex)
                    .ToList()
                    .Select(item => (item.Name ?? "").Trim())
                    .Where(name => name.Length > 0)
                    .ToList();
            }
            catch (Exception)
            {
                return [];
            }
        }

        /// <summary>
        /// 当前步骤之前已经写出的出参。用来标左侧「前序已出参」。本步自己的不算。
        /// </summary>
        private static IReadOnlySet<string> PriorOutputs(FlowCard? flow, FlowStep step)
        {
            return OutputNames(flow, step, earlierOnly: true);
        }

        /// <summary>
        /// 其它步骤已经占用的出参。右侧不能再选这些名字。本步自己的不算。
        /// </summary>
        private static IReadOnlySet<string> OtherOutputs(FlowCard? flow, FlowStep step)
        {
            return OutputNames(flow, step, earlierOnly: false);
        }

        /// <summary>
        /// 收集出参名。earlierOnly 为真时只要当前步骤前面的。
        /// </summary>
        private static HashSet<string> OutputNames(FlowCard? flow, FlowStep step, bool earlierOnly)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            if (flow is null)
                return names;

            var index = flow.Steps.IndexOf(step);
            for (var i = 0; i < flow.Steps.Count; i++)
            {
                if (i == index)
                    continue;

                if (earlierOnly && i > index)
                    continue;

                foreach (var name in ToolVars.OutputNames(flow.Steps[i].ToolKind, flow.Steps[i].CfgValue))
                    names.Add(name);
            }

            return names;
        }

        /// <summary>
        /// 确认后删除这一步，并重写后面的顺序。取消则不删除。
        /// </summary>
        private async void DeleteStep(FlowStep? step)
        {
            if (LicenseGate.Block(ContainerExt))
                return;

            var flow = SelectedFlow;
            if (flow is null || step is null || !flow.Steps.Contains(step))
                return;

            SelectStep(step);
            var confirmed = await MetroDialogService.ConfirmAsync(
                Text("Flow_DeleteStepTitle", "删除步骤"),
                Text("Flow_DeleteMessage", "是否确认删除？"));
            if (!confirmed)
                return;

            var index = flow.Steps.IndexOf(step);
            var title = step.Title;
            flow.Steps.Remove(step);
            Renumber(flow.Steps);
            if (flow.Steps.Count > 0)
                SelectStep(flow.Steps[Math.Min(index, flow.Steps.Count - 1)]);

            if (!SqlSugarContext.IsOpen)
                return;

            try
            {
                var db = SqlSugarContext.CustomerDb;
                db.Ado.BeginTran();
                try
                {
                    db.Deleteable<FlowStepEntity>().Where(item => item.Id == step.Id).ExecuteCommand();
                    WriteOrder(db, flow);
                    db.Ado.CommitTran();
                }
                catch (Exception)
                {
                    db.Ado.RollbackTran();
                    throw;
                }

                LogHelper.FlowInfo($"删除流程步 {title}");
            }
            catch (Exception)
            {
                await MetroDialogService.ShowMessageAsync(
                    Text("Flow_DeleteStepTitle", "删除步骤"),
                    Text("Flow_DeleteFailed", "删除失败"));
                Load(flow.Id);
            }
        }

        /// <summary>
        /// 插入一步并按新顺序写回序号。失败时重新读库。
        /// </summary>
        private void InsertStep(FlowCard flow, FlowTool tool, int index)
        {
            var step = new FlowStep
            {
                Id = Guid.NewGuid(),
                ToolKind = tool.ToolKind,
                Kind = tool.Name,
                Title = tool.Name,
                Detail = "",
                CfgValue = ""
            };
            flow.Steps.Insert(index, step);
            Renumber(flow.Steps);
            if (!SqlSugarContext.IsOpen)
            {
                SelectStep(step);
                return;
            }

            try
            {
                var db = SqlSugarContext.CustomerDb;
                db.Insertable(new FlowStepEntity
                {
                    Id = step.Id,
                    FlowId = flow.Id,
                    SortIndex = index + 1,
                    ToolKind = tool.ToolKind,
                    Title = Limit(tool.Name, 32),
                    CfgValue = ""
                }).ExecuteCommand();
                WriteOrder(db, flow);
                SelectStep(step);
            }
            catch (Exception)
            {
                Load(flow.Id);
            }
        }

        /// <summary>
        /// 把属性窗口的结果写回这一步。
        /// </summary>
        private bool SaveProperty(FlowStep step, ToolEditResult result)
        {
            if (!SqlSugarContext.IsOpen)
                return false;

            try
            {
                var db = SqlSugarContext.CustomerDb;
                var entity = db.Queryable<FlowStepEntity>().Where(item => item.Id == step.Id).ToList().FirstOrDefault();
                if (entity is null)
                    return false;

                entity.Title = result.Title;
                entity.CfgValue = result.CfgValue;
                db.Updateable(entity).ExecuteCommand();
                step.Title = result.Title;
                step.Detail = result.Detail;
                step.CfgValue = result.CfgValue;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 在当前流程里移动一步，并写回执行顺序。位置没变时不写库。
        /// </summary>
        private void MoveStep(FlowCard flow, FlowStep step, int insertIndex)
        {
            var steps = flow.Steps;
            var from = steps.IndexOf(step);
            if (from < 0)
                return;

            var to = insertIndex;
            if (to > from)
                to--;
            if (to < 0)
                to = 0;
            if (to >= steps.Count)
                to = steps.Count - 1;
            if (to == from)
            {
                SelectStep(step);
                return;
            }

            steps.Move(from, to);
            Renumber(steps);
            if (!SqlSugarContext.IsOpen)
            {
                SelectStep(step);
                return;
            }

            try
            {
                WriteOrder(SqlSugarContext.CustomerDb, flow);
                SelectStep(step);
            }
            catch (Exception)
            {
                Load(flow.Id);
            }
        }

        /// <summary>
        /// 按当前列表重写序号和连线。
        /// </summary>
        private static void Renumber(IReadOnlyList<FlowStep> steps)
        {
            for (var i = 0; i < steps.Count; i++)
            {
                steps[i].Number = i + 1;
                steps[i].ShowStem = i < steps.Count - 1;
            }
        }

        /// <summary>
        /// 把列表顺序写回 sort_index。
        /// </summary>
        private static void WriteOrder(SqlSugarScope db, FlowCard flow)
        {
            var rows = db.Queryable<FlowStepEntity>().Where(item => item.FlowId == flow.Id).ToList();
            var byId = rows.ToDictionary(item => item.Id);
            for (var i = 0; i < flow.Steps.Count; i++)
            {
                if (!byId.TryGetValue(flow.Steps[i].Id, out var row))
                    continue;

                row.SortIndex = i + 1;
                db.Updateable(row).ExecuteCommand();
            }
        }
    }
}
