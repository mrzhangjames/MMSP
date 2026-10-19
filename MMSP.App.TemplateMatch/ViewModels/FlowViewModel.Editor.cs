using System.Windows;
using MMSP.App.TemplateMatch.Models;
using MMSP.App.TemplateMatch.Views;
using MMSP.App.WpfBase.Services;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.DB;
using MMSP.Service.Database;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 流程页的新增、编辑和删除。弹窗只改名称，开关仍在卡片上保存。
    /// </summary>
    public partial class FlowViewModel
    {
        /// <summary>
        /// 打开空白的新增弹窗。
        /// </summary>
        private void OpenAdd()
        {
            EditName(null, "", "");
        }

        /// <summary>
        /// 打开编辑弹窗并带回这张卡片的名称。没有目标时不打开。
        /// </summary>
        private void OpenEdit(FlowCard? card)
        {
            card ??= SelectedFlow;
            if (card is null)
                return;

            Select(card);
            EditName(card.Id, card.Name, "");
        }

        /// <summary>
        /// 用 Metro 窗口收集名称。保存失败时带着错误重新打开。
        /// </summary>
        private void EditName(Guid? id, string name, string error)
        {
            var title = id is null
                ? Text("Flow_AddTitle", "新增流程")
                : Text("Flow_EditTitle", "编辑流程");
            var window = new FlowNameWindow(title, name, error);
            var owner = Application.Current?.MainWindow;
            if (owner is not null && owner.IsLoaded)
                window.Owner = owner;

            if (window.ShowDialog() != true || window.Result is not string saved)
                return;

            if (!CommitName(id, saved))
                EditName(id, saved, Text("User_SaveFailed", "保存失败"));
        }

        /// <summary>
        /// 把名称写入客户库。成功后刷新列表。
        /// </summary>
        private bool CommitName(Guid? id, string name)
        {
            if (!SqlSugarContext.IsOpen)
                return false;

            try
            {
                var db = SqlSugarContext.CustomerDb;
                Guid savedId;
                if (id is Guid existing)
                {
                    var entity = db.Queryable<FlowEntity>().Where(item => item.Id == existing).ToList().FirstOrDefault();
                    if (entity is null)
                        return false;

                    entity.Name = name;
                    db.Updateable(entity).ExecuteCommand();
                    savedId = existing;
                    LogHelper.FlowInfo($"编辑流程 {name}");
                }
                else
                {
                    var indexes = db.Queryable<FlowEntity>().Select(item => item.SortIndex).ToList();
                    savedId = Guid.NewGuid();
                    db.Insertable(new FlowEntity
                    {
                        Id = savedId,
                        Name = name,
                        IsEnabled = true,
                        LogOutput = false,
                        SortIndex = indexes.Count == 0 ? 1 : indexes.Max() + 1
                    }).ExecuteCommand();
                    LogHelper.FlowInfo($"新增流程 {name}");
                }

                Load(savedId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 确认后删除当前流程及其流程步。取消则不删除。
        /// </summary>
        private async void Delete()
        {
            var card = SelectedFlow;
            if (card is null || !SqlSugarContext.IsOpen)
                return;

            var confirmed = await MetroDialogService.ConfirmAsync(
                Text("Flow_DeleteTitle", "删除流程"),
                Text("Flow_DeleteMessage", "是否确认删除？"));
            if (!confirmed)
                return;

            var id = card.Id;
            var name = card.Name;
            var index = IndexOf(id);
            Guid? next = null;
            if (index >= 0 && index + 1 < Flows.Count)
                next = Flows[index + 1].Id;
            else if (index > 0)
                next = Flows[index - 1].Id;

            try
            {
                var db = SqlSugarContext.CustomerDb;
                db.Ado.BeginTran();
                try
                {
                    db.Deleteable<FlowStepEntity>().Where(item => item.FlowId == id).ExecuteCommand();
                    db.Deleteable<FlowEntity>().Where(item => item.Id == id).ExecuteCommand();
                    db.Ado.CommitTran();
                }
                catch (Exception)
                {
                    db.Ado.RollbackTran();
                    throw;
                }

                LogHelper.FlowInfo($"删除流程 {name}");
                Load(next);
            }
            catch (Exception)
            {
                await MetroDialogService.ShowMessageAsync(
                    Text("Flow_DeleteTitle", "删除流程"),
                    Text("Flow_DeleteFailed", "删除失败"));
            }
        }

        /// <summary>
        /// 当前流程在列表中的位置。找不到时返回 -1。
        /// </summary>
        private int IndexOf(Guid id)
        {
            for (var i = 0; i < Flows.Count; i++)
            {
                if (Flows[i].Id == id)
                    return i;
            }

            return -1;
        }
    }
}
