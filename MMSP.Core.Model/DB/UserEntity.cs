using MMSP.Core.Model.Base;
using MMSP.Core.Model.Sys;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace MMSP.Core.Model.DB
{
    /// <summary>
    /// 用户实体类
    /// </summary>
    [SugarTable("sys_users")]
    public class UserEntity : BaseDBEntity
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [SugarColumn(ColumnName = "user_name")]
        public string? UserName { get; set; }

        /// <summary>
        /// 登录账户
        /// </summary>
        [SugarColumn(ColumnName = "login_name")]
        public string? LoginName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [SugarColumn(ColumnName = "password")]
        public string? Password { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        [SugarColumn(ColumnName = "role")]
        public UserRole RoleLevel { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(ColumnName = "is_valid")]
        public bool IsValid { get; set; }

        /// <summary>
        /// TODO:会话超时时间（分钟），0：表示不超时
        /// </summary>
        [SugarColumn(ColumnName = "session_timeout")]
        public int SessionTimeout { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnName = "create_time")]
        public DateTime CreateTime { get; set; }
    }
}
