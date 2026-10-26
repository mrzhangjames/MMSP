using System.Globalization;
using MMSP.App.TemplateMatch.Models;
using MMSP.App.WpfBase.Services;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.DB;
using MMSP.Service.Database;
using MMSP.Service.Flow;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 流程页的单次执行、单步执行和复制。执行只跑当前选中的一条，单步只跑右键的一步，跑完即停。复制连同流程步和变量一起新增。
    /// </summary>
    public partial class FlowViewModel
    {
        /// <summary>
        /// 流程名称最长 32。和名称窗口、库字段一致。
        /// </summary>
        private const int FlowNameMax = 32;

        /// <summary>
        /// 单次执行进行中。避免连点再开一轮。
        /// </summary>
        private int _onceBusy;

        /// <summary>
        /// 执行当前流程一轮。没有选中、库没打开或已经在跑时不开始。
        /// 不论启用开关。日志输出打开时，成功步骤的耗时和结果由执行服务写入。
        /// </summary>
        private async void RunSelected()
        {
            if (LicenseGate.Block(ContainerExt))
                return;

            var card = SelectedFlow;
            if (card is null || !SqlSugarContext.IsOpen)
                return;

            if (Interlocked.Exchange(ref _onceBusy, 1) == 1)
                return;

            var cleared = 0;
            var progress = new Progress<FlowStepProgress>(mark =>
            {
                if (Interlocked.Exchange(ref cleared, 1) == 0)
                    ClearRun(card);

                ShowRun(card, mark);
            });

            try
            {
                await _flowRun.RunOnceAsync(card.Id, progress);
            }
            catch (Exception ex)
            {
                LogHelper.FlowError("流程执行失败", ex);
            }
            finally
            {
                Interlocked.Exchange(ref _onceBusy, 0);
            }
        }

        /// <summary>
        /// 只跑右键的这一步。不清除其它步骤上一次留下的状态。
        /// 前面的步骤不重跑，变量仍是初始值。日志输出打开时，成功的耗时和结果由执行服务写入。
        /// </summary>
        private async void RunStep(FlowStep? step)
        {
            if (LicenseGate.Block(ContainerExt))
                return;

            var card = SelectedFlow;
            if (step is null || card is null || !card.Steps.Contains(step) || !SqlSugarContext.IsOpen)
                return;

            if (Interlocked.Exchange(ref _onceBusy, 1) == 1)
                return;

            step.ClearRun();
            var progress = new Progress<FlowStepProgress>(mark => ShowRun(card, mark));
            try
            {
                await _flowRun.RunStepAsync(card.Id, step.Id, progress);
            }
            catch (Exception ex)
            {
                LogHelper.FlowError("流程执行失败", ex);
            }
            finally
            {
                Interlocked.Exchange(ref _onceBusy, 0);
            }
        }

        /// <summary>
        /// 把通知写到对应的步骤卡片上。找不到这一步时不改其它卡片。
        /// </summary>
        private static void ShowRun(FlowCard card, FlowStepProgress mark)
        {
            var step = card.Steps.FirstOrDefault(item => mark.StepId != Guid.Empty && item.Id == mark.StepId)
                ?? card.Steps.FirstOrDefault(item => item.Number == mark.SortIndex);
            if (step is null)
                return;

            var elapsed = mark.State is FlowStepRunState.Running or FlowStepRunState.Skipped or FlowStepRunState.None
                ? ""
                : FlowStepProgress.FormatElapsed(mark.ElapsedMs);
            step.Mark((int)mark.State, StateText(mark.State), elapsed, mark.Detail);
        }

        /// <summary>
        /// 清掉上一轮留在卡片上的状态和耗时。
        /// </summary>
        private static void ClearRun(FlowCard card)
        {
            foreach (var step in card.Steps)
                step.ClearRun();
        }

        /// <summary>
        /// 状态在卡片上的文字。
        /// </summary>
        private static string StateText(FlowStepRunState state)
        {
            return state switch
            {
                FlowStepRunState.Running => Text("Flow_RunRunning", "执行中"),
                FlowStepRunState.Ok => Text("Flow_RunOk", "成功"),
                FlowStepRunState.Failed => Text("Flow_RunFailed", "失败"),
                FlowStepRunState.Skipped => Text("Flow_RunSkipped", "跳过"),
                FlowStepRunState.Cancelled => Text("Flow_RunCancelled", "已取消"),
                _ => ""
            };
        }

        /// <summary>
        /// 复制当前流程、它的流程步和变量。新卡片排在列表末尾并被选中。
        /// </summary>
        private async void CopySelected()
        {
            if (LicenseGate.Block(ContainerExt))
                return;

            var card = SelectedFlow;
            if (card is null || !SqlSugarContext.IsOpen)
                return;

            try
            {
                var saved = CopyFlow(card.Id);
                if (saved is null)
                    return;

                LogHelper.FlowInfo($"复制流程 {saved.Value.Source} 为 {saved.Value.Name}");
                Load(saved.Value.Id);
            }
            catch (Exception)
            {
                await MetroDialogService.ShowMessageAsync(
                    Text("Flow_Copy", "复制"),
                    Text("Flow_CopyFailed", "复制失败"));
            }
        }

        /// <summary>
        /// 把一条流程抄成新的主键。名称带「副本」，最长 32。启用和日志输出保持原样。
        /// 源流程已经不在时返回空。
        /// </summary>
        private static (Guid Id, string Name, string Source)? CopyFlow(Guid sourceId)
        {
            var db = SqlSugarContext.CustomerDb;
            var source = db.Queryable<FlowEntity>().Where(item => item.Id == sourceId).ToList().FirstOrDefault();
            if (source is null)
                return null;

            var steps = db.Queryable<FlowStepEntity>()
                .Where(item => item.FlowId == sourceId)
                .OrderBy(item => item.SortIndex)
                .ToList();
            var variables = db.Queryable<FlowVarEntity>()
                .Where(item => item.FlowId == sourceId)
                .OrderBy(item => item.SortIndex)
                .ToList();
            var names = db.Queryable<FlowEntity>().Select(item => item.Name).ToList();
            var indexes = db.Queryable<FlowEntity>().Select(item => item.SortIndex).ToList();
            var id = Guid.NewGuid();
            var name = CopyName(source.Name, names);
            var flow = new FlowEntity
            {
                Id = id,
                Name = name,
                IsEnabled = source.IsEnabled,
                LogOutput = source.LogOutput,
                SortIndex = indexes.Count == 0 ? 1 : indexes.Max() + 1
            };

            db.Ado.BeginTran();
            try
            {
                db.Insertable(flow).ExecuteCommand();
                if (steps.Count > 0)
                {
                    var copies = new List<FlowStepEntity>(steps.Count);
                    foreach (var step in steps)
                    {
                        copies.Add(new FlowStepEntity
                        {
                            Id = Guid.NewGuid(),
                            FlowId = id,
                            SortIndex = step.SortIndex,
                            ToolKind = step.ToolKind,
                            Title = step.Title ?? "",
                            CfgValue = step.CfgValue ?? ""
                        });
                    }

                    db.Insertable(copies).ExecuteCommand();
                }

                if (variables.Count > 0)
                {
                    var copies = new List<FlowVarEntity>(variables.Count);
                    foreach (var item in variables)
                    {
                        copies.Add(new FlowVarEntity
                        {
                            Id = Guid.NewGuid(),
                            FlowId = id,
                            Name = item.Name ?? "",
                            DataType = item.DataType ?? "",
                            InitialValue = item.InitialValue ?? "",
                            SortIndex = item.SortIndex
                        });
                    }

                    db.Insertable(copies).ExecuteCommand();
                }

                db.Ado.CommitTran();
            }
            catch (Exception)
            {
                db.Ado.RollbackTran();
                throw;
            }

            return (id, name, source.Name ?? "");
        }

        /// <summary>
        /// 生成不重复的副本名称。已有「名称 副本」时改用「名称 副本2」。
        /// </summary>
        private static string CopyName(string source, IReadOnlyList<string> names)
        {
            var taken = new HashSet<string>(StringComparer.Ordinal);
            foreach (var name in names)
            {
                var text = (name ?? "").Trim();
                if (text.Length > 0)
                    taken.Add(text);
            }

            var stem = (source ?? "").Trim();
            if (stem.Length == 0)
                stem = "流程";

            var first = FitName(stem, " 副本");
            if (!taken.Contains(first))
                return first;

            for (var index = 2; index < 100; index++)
            {
                var candidate = FitName(stem, " 副本" + index.ToString(CultureInfo.InvariantCulture));
                if (!taken.Contains(candidate))
                    return candidate;
            }

            return first;
        }

        /// <summary>
        /// 把后缀接在名称后面，总长不超过 32。名称过长时先截名称。
        /// </summary>
        private static string FitName(string stem, string suffix)
        {
            var room = FlowNameMax - suffix.Length;
            if (room < 1)
                return suffix.Length <= FlowNameMax ? suffix : suffix[..FlowNameMax];

            var head = stem.Length <= room ? stem : stem[..room];
            return head + suffix;
        }
    }
}
