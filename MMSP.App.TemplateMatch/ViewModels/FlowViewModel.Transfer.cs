using System.IO;
using System.Text;
using Microsoft.Win32;
using MMSP.App.WpfBase.Services;
using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Tool;
using MMSP.Service.Database;
using SqlSugar;

namespace MMSP.App.TemplateMatch.ViewModels
{
    /// <summary>
    /// 流程页的全部配置导入导出。
    /// </summary>
    public partial class FlowViewModel
    {
        /// <summary>
        /// 导入或导出进行中，避免连点再开一个对话框。
        /// </summary>
        private bool _transferBusy;

        /// <summary>
        /// 把客户库里的全部流程、流程步和变量写成一个 JSON 文件。
        /// </summary>
        private async void ExportFlows()
        {
            if (_transferBusy)
                return;

            if (!SqlSugarContext.IsOpen)
            {
                await MetroDialogService.ShowMessageAsync(Text("Flow_Export", "导出"), Text("Flow_ExportFailed", "导出失败"));
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "JSON (*.json)|*.json",
                FileName = "flows.json",
                DefaultExt = ".json",
                AddExtension = true
            };
            if (dialog.ShowDialog() != true)
                return;

            _transferBusy = true;
            try
            {
                var db = SqlSugarContext.CustomerDb;
                var json = FlowFile.Write(
                    db.Queryable<FlowEntity>().ToList(),
                    db.Queryable<FlowStepEntity>().ToList(),
                    db.Queryable<FlowVarEntity>().ToList());
                File.WriteAllText(dialog.FileName, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                LogHelper.FlowInfo("导出流程配置");
            }
            catch (SqlSugarException ex)
            {
                LogHelper.FlowError("导出流程配置失败", ex);
                await MetroDialogService.ShowMessageAsync(Text("Flow_Export", "导出"), Text("Flow_ExportFailed", "导出失败"));
            }
            catch (ArgumentException ex)
            {
                LogHelper.FlowError("导出流程配置失败", ex);
                await MetroDialogService.ShowMessageAsync(Text("Flow_Export", "导出"), Text("Flow_ExportFailed", "导出失败"));
            }
            catch (UnauthorizedAccessException ex)
            {
                LogHelper.FlowError("导出流程配置失败", ex);
                await MetroDialogService.ShowMessageAsync(Text("Flow_Export", "导出"), Text("Flow_ExportFailed", "导出失败"));
            }
            catch (IOException ex)
            {
                LogHelper.FlowError("导出流程配置失败", ex);
                await MetroDialogService.ShowMessageAsync(Text("Flow_Export", "导出"), Text("Flow_ExportFailed", "导出失败"));
            }
            finally
            {
                _transferBusy = false;
            }
        }

        /// <summary>
        /// 先校验文件。确认后在一个事务里用新 Id 替换全部流程、流程步和变量。取消或失败时库和列表不变。
        /// </summary>
        private async void ImportFlows()
        {
            if (LicenseGate.Block(ContainerExt) || _transferBusy)
                return;

            var dialog = new OpenFileDialog
            {
                Filter = "JSON (*.json)|*.json",
                CheckFileExists = true
            };
            if (dialog.ShowDialog() != true)
                return;

            _transferBusy = true;
            try
            {
                string json;
                try
                {
                    json = File.ReadAllText(dialog.FileName);
                }
                catch (ArgumentException ex)
                {
                    LogHelper.FlowError("读取流程配置失败", ex);
                    await MetroDialogService.ShowMessageAsync(Text("Flow_Import", "导入"), Text("Flow_ImportFailed", "导入失败"));
                    return;
                }
                catch (UnauthorizedAccessException ex)
                {
                    LogHelper.FlowError("读取流程配置失败", ex);
                    await MetroDialogService.ShowMessageAsync(Text("Flow_Import", "导入"), Text("Flow_ImportFailed", "导入失败"));
                    return;
                }
                catch (IOException ex)
                {
                    LogHelper.FlowError("读取流程配置失败", ex);
                    await MetroDialogService.ShowMessageAsync(Text("Flow_Import", "导入"), Text("Flow_ImportFailed", "导入失败"));
                    return;
                }

                if (!FlowFile.TryRead(json, out var flows, out var error))
                {
                    await MetroDialogService.ShowMessageAsync(Text("Flow_Import", "导入"), error);
                    return;
                }

                if (!SqlSugarContext.IsOpen)
                {
                    await MetroDialogService.ShowMessageAsync(Text("Flow_Import", "导入"), Text("Flow_ImportFailed", "导入失败"));
                    return;
                }

                var confirmed = await MetroDialogService.ConfirmAsync(
                    Text("Flow_ImportTitle", "导入流程"),
                    Text("Flow_ImportMessage", "将替换全部流程配置。是否继续？"));
                if (!confirmed)
                    return;

                if (!ReplaceFlows(flows))
                {
                    await MetroDialogService.ShowMessageAsync(Text("Flow_Import", "导入"), Text("Flow_ImportFailed", "导入失败"));
                    return;
                }

                LogHelper.FlowInfo("导入流程配置");
                Load(null);
            }
            finally
            {
                _transferBusy = false;
            }
        }

        /// <summary>
        /// 删掉三张表的全部行，再插入文件里的流程。不含 MES。失败则回滚。
        /// </summary>
        private static bool ReplaceFlows(IReadOnlyList<FlowFileFlow> flows)
        {
            FlowFile.CreateRows(flows, out var flowRows, out var stepRows, out var varRows);
            var db = SqlSugarContext.CustomerDb;
            db.Ado.BeginTran();
            try
            {
                db.Ado.ExecuteCommand("DELETE FROM biz_flow_vars");
                db.Ado.ExecuteCommand("DELETE FROM biz_flow_steps");
                db.Ado.ExecuteCommand("DELETE FROM biz_flows");
                if (flowRows.Count > 0)
                    db.Insertable(flowRows).ExecuteCommand();
                if (stepRows.Count > 0)
                    db.Insertable(stepRows).ExecuteCommand();
                if (varRows.Count > 0)
                    db.Insertable(varRows).ExecuteCommand();

                db.Ado.CommitTran();
                return true;
            }
            catch (SqlSugarException ex)
            {
                LogHelper.FlowError("导入流程配置失败", ex);
                db.Ado.RollbackTran();
                return false;
            }
        }
    }
}
