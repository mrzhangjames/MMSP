using MMSP.Core.Model.Base;
using MMSP.Core.Model.DB;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace MMSP.Service.Database
{
    public class SqlSugarContext : IDisposable
    {
        public static SqlSugarScope Db = new SqlSugarScope(new ConnectionConfig()
        {
            DbType = SqlSugar.DbType.Sqlite,// 数据库类型
            ConnectionString = PathCore.ConnectionString_Sqlite,
            LanguageType = LanguageType.Default,//中&英
            IsAutoCloseConnection = true// 是否自动关闭数据库连接
        },
         db =>
         {
             //单例参数配置，所有上下文生效
             db.Aop.OnLogExecuting = (s, p) =>
             {
                 //writeLog(s, p);
                 Console.WriteLine(s);
             };

             //Sql超时
             db.Ado.CommandTimeOut = 10;//单位秒

             // 建库：如果不存在创建数据库,存在不会重复创建 createdb
             db.DbMaintenance.CreateDatabase();
             db.CodeFirst.InitTables(typeof(UserEntity));
             db.CodeFirst.InitTables(typeof(MenuEntity));
         });

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
