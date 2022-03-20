using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//C/S共用，常更改，放一块方便拷贝

/// <summary>注册方式</summary>
public enum RegisterType
{
    Phone,
    Mail,
   //微信
   //QQ 
}
/// <summary>登录类型</summary>
public enum LoginType
{
    Phone,
    Mail,
    WX,
    QQ,
    /// <summary>用以自动类型</summary>
    Token,
    NULL
}
/// <summary>注册返回值</summary>
public enum RegisterResult
{
    Success,
    Failed,
    /// <summary>账号已存在</summary>
    AlreadyExist,
    /// <summary>比如验证码错误</summary>
    WrongCode,
    /// <summary>比如封账号</summary>
    Forbidden,
    //验证码，阿里云
}
/// <summary>登录结果</summary>
public enum LoginResult
{
    Success,
    Failed,
    WrongPwd,
    UserNotExist,
    /// <summary>token超时过期</summary>
    TimeoutToken,
}

