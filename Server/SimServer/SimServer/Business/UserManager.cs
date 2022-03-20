using MySql;
using MySql.MySQLData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimServer.Business
{
    public class UserManager : Singleton<UserManager>
    {


        #region 辅助1
        /// <summary>
        /// 注册，正常情况下，还要包含检测验证码是否正确
        /// </summary>
        /// <param name="registerType">注册类型</param>
        /// <param name="userName">用户名</param>
        /// <param name="pwd">密码</param>
        /// <param name="token">自动登录</param>
        /// <returns></returns>
        public RegisterResult Register(RegisterType registerType, string userName, string pwd, out string token)
        {
            token = "";
            try
            {
                int count = MySqlMgr.Instance.SqlSugarDB.Queryable<User>().Where(it => it.Username == userName).Count();
                if (count > 0)
                    return RegisterResult.AlreadyExist;
                //
                InitUser(userName, pwd, out token, out User user);
                //
                switch (registerType)
                {
                    //不算是手机验证码或者邮箱验证码，再注册之前会有一个协议来申请验证码
                    //申请的验证码生成后在数据库储存一份，然后在注册的时候把客户端传入的验证码与数据库的验证码进行比较
                    //如果不一致，证明注册验证码错误，返回  RegisterResult.WrongCode
                    case RegisterType.Phone:
                        user.Logintype = LoginType.Phone.ToString();
                        break;
                    case RegisterType.Mail:
                        user.Logintype = LoginType.Mail.ToString();
                        break;
                }
                MySqlMgr.Instance.SqlSugarDB.Insertable(user).ExecuteCommand();//user必须是无参构造
                //
                return RegisterResult.Success;
            }
            catch (Exception ex)
            {
                Debug.LogError("注册失败：" + ex.ToString());
                return RegisterResult.Failed;
            }
        }



        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginType"></param>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <param name="userid">匹配数据库</param>
        /// <param name="token"></param>
        /// <returns></returns>

        public LoginResult Login(LoginType loginType, string userName, string pwd, out int userid, out string token)
        {
            userid = 0;
            token = "";
            try
            {
                GetUserFromDB(loginType, userName, out User user);
                //
                if (user == null)
                {
                    //QQ微信是首次登录的话相当于注册
                    if (loginType == LoginType.QQ || loginType == LoginType.WX)
                    {
                        //在数据库注册QQWX
                        InitUser(userName, pwd, out token, out user);
                        user.Logintype = loginType.ToString();
                        //
                        userid = MySqlMgr.Instance.SqlSugarDB.Insertable(user).ExecuteReturnIdentity();
                        //
                        return LoginResult.Success;
                    }
                    else
                    {
                        return LoginResult.UserNotExist;
                    }
                }
                else
                {
                    if (loginType != LoginType.Token)
                    {
                        if (loginType == LoginType.Phone)
                        {
                            if (user.Password != pwd)
                                return LoginResult.WrongPwd;
                        }
                        else if (loginType == LoginType.Mail)
                        {
                            if (user.Password != pwd)
                                return LoginResult.WrongPwd;
                        }
                    }
                    else
                    {
                        if (user.Token != pwd)
                            return LoginResult.TimeoutToken;//过期
                    }
                    //
                    user.Token = Guid.NewGuid().ToString();
                    user.Logindate = DateTime.Now;
                    MySqlMgr.Instance.SqlSugarDB.Updateable(user).ExecuteCommand();
                    //
                    token = user.Token;
                    userid = user.Id;//比如新用户登录
                    //
                    return LoginResult.Success;
                }


            }
            catch (Exception ex)
            {
                Debug.LogError("登录失败：" + ex.ToString());
                return LoginResult.Failed;
            }
        }
        #endregion




        #region 辅助2
        /// <summary>
        /// 从数据库获取User
        /// </summary>
        /// <param name="loginType"></param>
        /// <param name="userName"></param>
        /// <param name="user"></param>
        void GetUserFromDB(LoginType loginType, string userName, out User user)
        {
            user = null;
            switch (loginType)
            {
                //用UserName索引
                case LoginType.Phone:
                case LoginType.Mail:
                    user = MySqlMgr.Instance.SqlSugarDB.Queryable<User>().Where(it
                        => it.Username == userName).Single();
                    break;
                //用Unionid索引
                case LoginType.QQ:
                case LoginType.WX:
                    break;
                //加载token，用useName索引
                case LoginType.Token:
                    user = MySqlMgr.Instance.SqlSugarDB.Queryable<User>().Where(it
                        => it.Username == userName).Single();
                    break;
            }
        }


        /// <summary>
        /// 生成User信息
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <param name="token"></param>
        /// <param name="user"></param>
        /// <returns>User</returns>
        User InitUser( string userName, string pwd, out string token, out User user)
        {
            user = new User();

            user.Username = userName;
            user.Password = pwd;
            user.Token = Guid.NewGuid().ToString();
            token = user.Token;
            user.Logindate = DateTime.Now;

            return user;
        }
        #endregion

    }
}
