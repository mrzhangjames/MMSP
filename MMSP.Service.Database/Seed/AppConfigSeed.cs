using MMSP.Core.Model.DB;
using SqlSugar;

namespace MMSP.Service.Database.Seed
{
    /// <summary>
    /// 保证系统库里有一行界面配置。已有行不覆盖用户改过的值。
    /// </summary>
    public static class AppConfigSeed
    {
        /// <summary>
        /// 没有配置行时插入默认值。
        /// </summary>
        public static void Ensure(SqlSugarScope db)
        {
            var exists = db.Queryable<AppConfigEntity>().Where(item => item.Id == AppConfigEntity.RowId).Any();
            if (exists)
                return;

            db.Insertable(AppConfigEntity.CreateDefault()).ExecuteCommand();
        }
    }
}
