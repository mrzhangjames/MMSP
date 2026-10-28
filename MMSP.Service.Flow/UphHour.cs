using MMSP.Common.Helper.Logger;
using MMSP.Core.Model.DB;
using SqlSugar;

namespace MMSP.Service.Flow
{
    /// <summary>
    /// 给当前小时的 biz_uph 记 1。同一小时一行，更早小时的累计产量不改。
    /// </summary>
    internal static class UphHour
    {
        /// <summary>
        /// 按本机时间所在小时累加。ok 为 true 时良品数加 1，否则不良数加 1。小时产量都加 1。
        /// 该小时的累计产量是当天 0 点至该小时的小时产量之和。失败时返回原因，不留下半行。
        /// </summary>
        public static string? Add(ISqlSugarClient db, DateTime now, bool ok)
        {
            ArgumentNullException.ThrowIfNull(db);
            var hour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Unspecified);
            var day = hour.Date;
            Exception? last = null;
            for (var attempt = 0; attempt < 2; attempt++)
            {
                try
                {
                    db.Ado.BeginTran();
                    var row = db.Queryable<UphEntity>().Where(item => item.HourTime == hour).ToList().FirstOrDefault();
                    if (row is null)
                    {
                        row = new UphEntity
                        {
                            Id = Guid.NewGuid(),
                            HourTime = hour
                        };
                        db.Insertable(row).ExecuteCommand();
                    }

                    row.HourOutput += 1;
                    if (ok)
                        row.GoodCount += 1;
                    else
                        row.DefectCount += 1;

                    var earlier = db.Queryable<UphEntity>()
                        .Where(item => item.HourTime >= day && item.HourTime < hour)
                        .ToList()
                        .Sum(item => item.HourOutput);
                    row.TotalOutput = earlier + row.HourOutput;
                    db.Updateable(row)
                        .UpdateColumns(item => new { item.HourOutput, item.GoodCount, item.DefectCount, item.TotalOutput })
                        .ExecuteCommand();
                    db.Ado.CommitTran();
                    return null;
                }
                catch (SqlSugarException ex)
                {
                    last = ex;
                    Rollback(db);
                }
            }

            if (last is not null)
                LogHelper.FlowRunError("写入产量失败", last);

            return "写入失败";
        }

        /// <summary>
        /// 回滚这一次写入。回滚本身失败时不再向外抛，调用方按写入失败处理。
        /// </summary>
        private static void Rollback(ISqlSugarClient db)
        {
            try
            {
                db.Ado.RollbackTran();
            }
            catch (SqlSugarException ex)
            {
                LogHelper.FlowRunError("回滚产量写入失败", ex);
            }
        }
    }
}
