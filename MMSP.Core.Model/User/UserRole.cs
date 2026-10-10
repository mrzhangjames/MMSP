using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MMSP.Core.Model.User
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
}
