using System.ComponentModel;
using System.Reflection;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using SqlSugar;

namespace MMSP.Service.Database.Seed
{
    /// <summary>
    /// 按 <see cref="MenuCode"/> 把当前主界面菜单写入系统库。已有编码不改名称和图标，但会把最低角色写成枚举上的值。
    /// </summary>
    public static class MenuSeed
    {
        /// <summary>
        /// 补齐还没有的菜单行，并把已有行的最低角色写成枚举特性。
        /// </summary>
        public static void Ensure(SqlSugarScope db)
        {
            var existing = db.Queryable<MenuEntity>().Select(item => item.Code).ToList();
            var missing = Build().Where(item => !existing.Contains(item.Code)).ToList();
            if (missing.Count > 0)
                db.Insertable(missing).ExecuteCommand();

            ApplyMinRoles(db);
        }

        /// <summary>
        /// 按菜单编码更新最低角色。不改名称、图标、启用和排序。
        /// </summary>
        private static void ApplyMinRoles(SqlSugarScope db)
        {
            foreach (var code in Enum.GetValues<MenuCode>())
            {
                var minRole = ReadRole(code);
                db.Updateable<MenuEntity>()
                    .SetColumns(item => item.MinRole == minRole)
                    .Where(item => item.Code == code)
                    .ExecuteCommand();
            }
        }

        /// <summary>
        /// 从枚举生成六条菜单。
        /// </summary>
        private static List<MenuEntity> Build()
        {
            var rows = new List<MenuEntity>();
            foreach (var code in Enum.GetValues<MenuCode>())
                rows.Add(Create(code));
            return rows;
        }

        /// <summary>
        /// 生成一条菜单。名称用枚举说明，页面名用语言键。
        /// </summary>
        private static MenuEntity Create(MenuCode code)
        {
            var icons = ReadIcons(code);
            return new MenuEntity
            {
                Id = Guid.NewGuid(),
                ParentId = 0,
                IsValid = true,
                Code = code,
                Name = ReadDescription(code),
                PageName = "Nav_" + code,
                SelectedIcon = icons.SelectedIcon,
                UnSelectedIcon = icons.UnselectedIcon,
                SortIndex = (int)code,
                MinRole = ReadRole(code)
            };
        }

        /// <summary>
        /// 读取枚举上的最低角色。没有特性时抛出。
        /// </summary>
        private static UserRole ReadRole(MenuCode code)
        {
            return ReadField(code)?.GetCustomAttribute<MenuRoleAttribute>()?.MinRole
                ?? throw new InvalidOperationException($"菜单 {code} 没有最低角色。");
        }

        /// <summary>
        /// 读取枚举上的中文名称。
        /// </summary>
        private static string ReadDescription(MenuCode code)
        {
            var description = ReadField(code)?.GetCustomAttribute<DescriptionAttribute>();
            return description?.Description ?? code.ToString();
        }

        /// <summary>
        /// 读取枚举上的图标名。
        /// </summary>
        private static MenuIconsAttribute ReadIcons(MenuCode code)
        {
            return ReadField(code)?.GetCustomAttribute<MenuIconsAttribute>()
                ?? throw new InvalidOperationException($"菜单 {code} 没有图标。");
        }

        /// <summary>
        /// 找到枚举字段。
        /// </summary>
        private static FieldInfo? ReadField(MenuCode code)
        {
            return typeof(MenuCode).GetField(code.ToString());
        }
    }
}
