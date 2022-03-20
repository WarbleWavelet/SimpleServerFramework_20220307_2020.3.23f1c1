using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySql
{
    /// <summary>管理类</summary>
    public class MySqlMgr : Singleton<MySqlMgr>
    {
#if DEBUG
        //private const string connectingStr = "server=localhost;uid=root;pwd=Fhy521121xuaner;database=ocean";
        //private const string connectingStr = "server=localhost;uid=root;pwd=mysql111;database=MysqlTest_20220318";
        private const string connectingStr = "server=localhost;uid=root;pwd=mysql111;database=ocean";
#else
        //对应服务器配置
        private const string connectingStr = "server=localhost;uid=root;pwd=Fhy521121xuaner;database=ocean";
#endif

        public SqlSugarClient SqlSugarDB = null;
        /// <summary>连接到数据库</summary>
        public void Init()
        {
            SqlSugarDB = new SqlSugarClient(
                new ConnectionConfig()
                {
                    ConnectionString = connectingStr,
                    DbType = DbType.MySql,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                });

#if DEBUG
            //用来打印Sql方便你调式    
            SqlSugarDB.Aop.OnLogExecuting = (sql, pars) =>
            {
                Console.WriteLine(sql + "\r\n" +
                    SqlSugarDB.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                Console.WriteLine();
            };
#endif
        }
    }
}
