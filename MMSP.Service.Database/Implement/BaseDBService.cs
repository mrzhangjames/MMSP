using MMSP.Core.Model.Base;
using MMSP.Service.Database.Interface;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MMSP.Service.Database.Implement
{
    public class BaseDBService<T> : IBaseDBService<T> where T : BaseDBEntity, new()
    {
        /// <summary>
        /// 当前服务使用的库。系统配置传 SystemDb，客户数据传 CustomerDb。
        /// </summary>
        private readonly ISqlSugarClient _db;

        /// <summary>
        /// 绑定到一个已经打开的库。
        /// </summary>
        public BaseDBService(ISqlSugarClient db)
        {
            _db = db;
        }

        public bool Add(T item)
        {
            return _db.Insertable(item).ExecuteCommand() > 0;
        }

        public async Task<bool> AddAsync(T item)
        {
            return await _db.Insertable(item).ExecuteCommandAsync() > 0;
        }

        public bool Delete(T item)
        {
            return _db.Deleteable(item).ExecuteCommand() > 0;
        }

        public bool Delete(Expression<Func<T, bool>> predicate)
        {
            return _db.Deleteable<T>(predicate).ExecuteCommand() > 0;
        }

        public bool Delete(string key)
        {
            return _db.Deleteable<T>().In(key).ExecuteCommand() > 0;
        }

        public async Task<bool> DeleteAsync(T item)
        {
            return await _db.Deleteable(item).ExecuteCommandAsync() > 0;
        }

        public bool TruncateTable()
        {
            return _db.DbMaintenance.TruncateTable<T>();
        }

        public bool Delete(int key)
        {
            return _db.Deleteable<T>().In(key).ExecuteCommand() > 0;
        }

        public T GetItem(Expression<Func<T, bool>> predicate)
        {
            return _db.Queryable<T>().First(predicate);
        }

        public async Task<T> GetItemAsync(Expression<Func<T, bool>> predicate)
        {
            T item = await _db.Queryable<T>().FirstAsync(predicate);
            return item;
        }

        public List<T> GetList()
        {
            return _db.Queryable<T>().ToList();
        }

        public async Task<List<T>> GetListAsync()
        {
            return await _db.Queryable<T>().ToListAsync();
        }

        public List<T> GetList(Expression<Func<T, bool>> predicate)
        {
            return _db.Queryable<T>().Where(predicate).ToList();
        }

        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate)
        {
            return await _db.Queryable<T>().Where(predicate).ToListAsync();
        }

        public bool Save(T item)
        {
            return _db.Storageable(item).ExecuteCommand() > 0;
        }

        public bool Save(List<T> items)
        {
            return _db.Storageable(items).ExecuteCommand() > 0;
        }

        public async Task<bool> SaveAsync(T item)
        {
            return await _db.Storageable(item).ExecuteCommandAsync() > 0;
        }

        public async Task<bool> SaveAsync(List<T> items)
        {
            return await _db.Storageable(items).ExecuteCommandAsync() > 0;
        }

        public async Task<bool> SaveAsync(T item, Expression<Func<T, bool>> predicate)
        {
            if (GetItem(predicate) != null)
            {
                return await UpdateAsync(item);
            }
            else
            {
                return await AddAsync(item);
            }
        }

        public bool Update(T item)
        {
            return _db.Updateable(item).ExecuteCommand() > 0;
        }

        public async Task<bool> UpdateAsync(T item)
        {
            return await _db.Updateable(item).ExecuteCommandAsync() > 0;
        }

        public bool Update(List<T> items, int pageSize)
        {
            return _db.Updateable(items).PageSize(pageSize).ExecuteCommand() > 0;
        }
    }
}
