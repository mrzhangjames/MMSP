using System.ComponentModel;
using System.Reflection;
using MMSP.Core.Model.DB;
using MMSP.Core.Model.Sys;
using MMSP.Service.Database;
using SqlSugar;

namespace MMSP.Service.Database.Seed
{
    /// <summary>
    /// 按 <see cref="UserRole"/> 补默认账号。已有登录名不覆盖。
    /// </summary>
    public static class UserSeed
    {
        /// <summary>
        /// 四个角色共用的初始密码。入库前哈希，列里不留明文。
        /// </summary>
        private const string InitialPassword = "123456";

        /// <summary>
        /// 补齐还没有的默认账号，并把仍是明文的口令改成哈希。
        /// </summary>
        public static void Ensure(SqlSugarScope db)
        {
            var existing = db.Queryable<UserEntity>().Select(item => item.LoginName).ToList();
            var taken = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in existing)
            {
                if (!string.IsNullOrWhiteSpace(name))
                    taken.Add(name.Trim());
            }

            var missing = Build().Where(item => item.LoginName is not null && !taken.Contains(item.LoginName)).ToList();
            if (missing.Count > 0)
                db.Insertable(missing).ExecuteCommand();

            UpgradePlainPasswords(db);
        }

        /// <summary>
        /// 已有账号的明文口令改成哈希。已经是哈希的不重算。空口令留着，登录不会通过。
        /// </summary>
        private static void UpgradePlainPasswords(SqlSugarScope db)
        {
            var rows = db.Queryable<UserEntity>().Select(item => new { item.Id, item.Password }).ToList();
            foreach (var row in rows)
            {
                if (string.IsNullOrEmpty(row.Password) || UserPassword.IsHashed(row.Password))
                    continue;

                var hashed = UserPassword.Hash(row.Password);
                db.Updateable<UserEntity>()
                    .SetColumns(item => item.Password == hashed)
                    .Where(item => item.Id == row.Id)
                    .ExecuteCommand();
            }
        }

        /// <summary>
        /// 每个角色一条。管理员登录名是 admin。
        /// </summary>
        private static List<UserEntity> Build()
        {
            var now = DateTime.Now;
            return
            [
                Create(UserRole.Admin, "admin", now),
                Create(UserRole.Operator, "operator", now),
                Create(UserRole.Engineer, "engineer", now),
                Create(UserRole.Supplier, "supplier", now)
            ];
        }

        /// <summary>
        /// 生成一条启用的默认用户。用户名用角色说明。
        /// </summary>
        private static UserEntity Create(UserRole role, string loginName, DateTime createTime)
        {
            return new UserEntity
            {
                Id = Guid.NewGuid(),
                LoginName = loginName,
                UserName = ReadDescription(role),
                Password = UserPassword.Hash(InitialPassword),
                RoleLevel = role,
                IsValid = true,
                SessionTimeout = 0,
                CreateTime = createTime
            };
        }

        /// <summary>
        /// 读取角色上的中文名称。
        /// </summary>
        private static string ReadDescription(UserRole role)
        {
            var field = typeof(UserRole).GetField(role.ToString());
            var description = field?.GetCustomAttribute<DescriptionAttribute>();
            return description?.Description ?? role.ToString();
        }
    }
}
