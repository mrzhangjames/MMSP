using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MMSP.Core.Model.Sys
{
    public enum UserRole : uint
    {
        /// <summary>
        /// 超级管理员
        /// </summary>
        [Description("管理员")]
        Admin = 0,

        /// <summary>
        /// 操作员/维护员
        /// </summary>
        [Description("操作员")]
        Operator,

        /// <summary>
        /// 工程师/技术员
        /// </summary>
        [Description("工程师")]
        Engineer,

        /// <summary>
        /// 供应商/厂商
        /// </summary>
        [Description("供应商")]
        Supplier
    }

    /// <summary>
    /// 判断登录角色能否打开某个菜单。比较用名单，不用枚举数值。
    /// </summary>
    public static class UserRoleAccess
    {
        /// <summary>
        /// 登录角色是否满足菜单要求的最低角色。不认识的要求返回 false。
        /// </summary>
        /// <param name="actual">当前登录角色。</param>
        /// <param name="required">菜单上的最低角色。</param>
        public static bool CanOpen(UserRole actual, UserRole required)
        {
            return required switch
            {
                UserRole.Operator => actual is UserRole.Admin or UserRole.Operator or UserRole.Engineer or UserRole.Supplier,
                UserRole.Engineer => actual is UserRole.Admin or UserRole.Engineer,
                UserRole.Supplier => actual is UserRole.Admin or UserRole.Supplier,
                UserRole.Admin => actual == UserRole.Admin,
                _ => false
            };
        }
    }
}
