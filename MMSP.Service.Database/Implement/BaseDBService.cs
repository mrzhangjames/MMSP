using MMSP.Core.Model.Base;
using MMSP.Service.Database.Interface;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MMSP.Service.Database.Implement
{
    public class BaseDBService<T> : IBaseDBService<T> where T : BaseDBEntity, new()
    {
        public BaseDBService()
        {
        }

        public bool Add(T item)
        {
            return SqlSugarContext.Db.Insertable(item).ExecuteCommand() > 0;
        }

        public async Task<bool> AddAsync(T item)
        {
            return await SqlSugarContext.Db.Insertable(item).ExecuteCommandAsync() > 0;
        }

        public bool Delete(T item)
        {
            return SqlSugarContext.Db.Deleteable(item).ExecuteCommand() > 0;
        }

        public bool Delete(Expression<Func<T, bool>> predicate)
        {
            return SqlSugarContext.Db.Deleteable<T>(predicate).ExecuteCommand() > 0;
        }

        public bool Delete(string key)
        {
            return SqlSugarContext.Db.Deleteable<T>().In(key).ExecuteCommand() > 0;
        }

        public async Task<bool> DeleteAsync(T item)
        {
            return await SqlSugarContext.Db.Deleteable(item).ExecuteCommandAsync() > 0;
        }

        public bool TruncateTable()
        {
            return SqlSugarContext.Db.DbMaintenance.TruncateTable<T>();
        }

        public bool Delete(int key)
        {
            return SqlSugarContext.Db.Deleteable<T>().In(key).ExecuteCommand() > 0;
        }

        public T GetItem(Expression<Func<T, bool>> predicate)
        {
            return SqlSugarContext.Db.Queryable<T>().First(predicate);
        }

        public async Task<T> GetItemAsync(Expression<Func<T, bool>> predicate)
        {
            T item = await SqlSugarContext.Db.Queryable<T>().FirstAsync(predicate);
            return item;
        }

        public List<T> GetList()
        {
            return SqlSugarContext.Db.Queryable<T>().ToList();
        }

        public async Task<List<T>> GetListAsync()
        {
            return await SqlSugarContext.Db.Queryable<T>().ToListAsync();
        }

        public List<T> GetList(Expression<Func<T, bool>> predicate)
        {
            return SqlSugarContext.Db.Queryable<T>().Where(predicate).ToList();
        }

        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate)
        {
            return await SqlSugarContext.Db.Queryable<T>().Where(predicate).ToListAsync();
        }

        public bool Save(T item)
        {
            return SqlSugarContext.Db.Storageable(item).ExecuteCommand() > 0;
        }

        public bool Save(List<T> items)
        {
            return SqlSugarContext.Db.Storageable(items).ExecuteCommand() > 0;
        }

        public async Task<bool> SaveAsync(T item)
        {
            return await SqlSugarContext.Db.Storageable(item).ExecuteCommandAsync() > 0;
        }

        public async Task<bool> SaveAsync(List<T> items)
        {
            return await SqlSugarContext.Db.Storageable(items).ExecuteCommandAsync() > 0;
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
            return SqlSugarContext.Db.Updateable(item).ExecuteCommand() > 0;
        }

        public async Task<bool> UpdateAsync(T item)
        {
            return await SqlSugarContext.Db.Updateable(item).ExecuteCommandAsync() > 0;
        }

        public bool Update(List<T> items, int pageSize)
        {
            return SqlSugarContext.Db.Updateable(items).PageSize(pageSize).ExecuteCommand() > 0;
        }
    }
}
