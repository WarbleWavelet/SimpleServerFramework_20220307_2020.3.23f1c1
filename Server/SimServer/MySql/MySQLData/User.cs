using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace MySql.MySQLData
{
    [SugarTable("user")]
    public class User
    {


        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        /// <summary>Key</summary>
        public int Id { get; set; }
        /// <summary>用户名</summary>
        public string Username { get; set; }
        /// <summary>密码</summary>
        public string Password { get; set; }
        /// <summary>登录时间</summary>
        public DateTime Logindate { get; set; }
        /// <summary>
        /// 登录方式<para />
        /// QQ<para />
        /// 微信<para />
        /// ......<para />
        /// </summary>
        public string Logintype { get; set; }
        /// <summary>
        /// 自动登录<para />
        /// 比如激活码才能登录页<para />
        /// 每次登陆都会变<para />
        /// </summary>
        public string Token { get; set; }
    }
}
