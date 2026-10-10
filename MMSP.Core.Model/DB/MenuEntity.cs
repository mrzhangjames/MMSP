using MMSP.Core.Model.Base;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace MMSP.Core.Model.DB
{

    /// <summary>
    /// 用户实体类
    /// </summary>
    [SugarTable("sys_menus")]
    public class MenuEntity : BaseDBEntity
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// 父节点Id
        /// </summary>
        [SugarColumn(ColumnName = "parent_id")]
        public int ParentId { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(ColumnName = "is_valid")]
        public bool IsValid { get; set; }

        /// <summary>
        /// 选中图标
        /// </summary>
        [SugarColumn(ColumnName = "selected_icon")]
        public string? SelectedIcon { get; set; }

        /// <summary>
        /// 未选中图标
        /// </summary>
        [SugarColumn(ColumnName = "unselected_icon")]
        public string? UnSelectedIcon { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [SugarColumn(ColumnName = "name")]
        public string? Name { get; set; }

        /// <summary>
        /// 页面名称
        /// </summary>
        [SugarColumn(ColumnName = "page_name")]
        public string? PageName { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [SugarColumn(ColumnName = "sort_index")]
        public int SortIndex { get; set; }

    }
}
