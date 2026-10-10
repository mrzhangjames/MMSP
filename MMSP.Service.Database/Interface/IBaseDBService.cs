using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MMSP.Service.Database.Interface
{
    public interface IBaseDBService<T> where T : class, new()
    {
        /// <summary>
        /// 获取实体
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        T GetItem(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 获取实体(异步)
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<T> GetItemAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <returns></returns>
        List<T> GetList();

        /// <summary>
        /// 获取实体列表(异步)
        /// </summary>
        /// <returns></returns>
        Task<List<T>> GetListAsync();

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        List<T> GetList(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 获取列表(异步)
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Delete(T item);

        /// <summary>
        /// 删除(异步)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(T item);

        /// <summary>
        /// 清空表
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool TruncateTable();

        /// <summary>
        /// 主键删除
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Delete(int key);

        /// <summary>
        /// 主键删除
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Delete(string key);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="predicate">删除条件</param>
        /// <returns></returns>
        bool Delete(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Add(T item);

        Task<bool> AddAsync(T item);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Update(T item);

        Task<bool> UpdateAsync(T item);

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="items"></param>
        /// <param name="pageSize">分页</param>
        /// <returns></returns>
        bool Update(List<T> items, int pageSize);

        /// <summary>
        /// 保存（存在更新，不存在添加）
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Save(T item);

        bool Save(List<T> items);

        /// <summary>
        /// 异步保存（存在更新，不存在添加）
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> SaveAsync(T item);

        /// <summary>
        /// 批量保存
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task<bool> SaveAsync(List<T> items);

        /// <summary>
        /// 异步保存（存在更新，不存在添加）
        /// </summary>
        /// <param name="item"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<bool> SaveAsync(T item, Expression<Func<T, bool>> predicate);
    }
}
