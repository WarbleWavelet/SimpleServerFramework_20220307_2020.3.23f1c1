﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
/// <summary>所有的协议收发的一个单独类</summary>
public class ProtocolMgr
{
    /// <summary>
    /// 密钥请求链接服务器的第一个请求
    /// </summary>
    public static void SecretRequest() 
    {
        MsgSecret msg = new MsgSecret();
        NetManager.Instance.SendMsg(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgSecret, (resmsg) =>
        {
            NetManager.Instance.SetKey(((MsgSecret)resmsg).Srcret);
            Debug.Log("获取密钥：" + ((MsgSecret)resmsg).Srcret);
        });
    }
    /// <summary>长点测试分包</summary>
    public static void SocketTest() 
    {
        MsgTest msg = new MsgTest();
        string longStr = "" +
            "jkdfjjkdjkdchjl,dchcjkqasdfghukasdhukasdgasyjdgaskdhaskdfgasjdgashjkdgjasdjasdklazxjckl.claskl/djl;askd;s" +
            "jkdfjjkdjkdchjl,dchcjkqasdfghukasdhukasdgasyjdgaskdhaskdfgasjdgashjkdgjasdjasdklazxjckl.claskl/djl;askd;s" +
            "jkdfjjkdjkdchjl,dchcjkqasdfghukasdhukasdgasyjdgaskdhaskdfgasjdgashjkdgjasdjasdklazxjckl.claskl/djl;askd;s" +
            "jkdfjjkdjkdchjl,dchcjkqasdfghukasdhukasdgasyjdgaskdhaskdfgasjdgashjkdgjasdjasdklazxjckl.claskl/djl;askd;s" +
            "jkdfjjkdjkdchjl,dchcjkqasdfghukasdhukasdgasyjdgaskdhaskdfgasjdgashjkdgjasdjasdklazxjckl.claskl/djl;askd;s" +
            "jkdfjjkdjkdchjl,dchcjkqasdfghukasdhukasdgasyjdgaskdhaskdfgasjdgashjkdgjasdjasdklazxjckl.claskl/djl;askd;s" +
            "jkdfjjkdjkdchjl,dchcjkqasdfghukasdhukasdgasyjdgaskdhaskdfgasjdgashjkdgjasdjasdklazxjckl.claskl/djl;askd;s" +
            "jkdfjjkdjkdchjl,dchcjkqasdfghukasdhukasdgasyjdgaskdhaskdfgasjdgashjkdgjasdjasdklazxjckl.claskl/djl;askd;s" 
           ;
        string shortStr = "Ocean";
        msg.ReqContent = shortStr;
        NetManager.Instance.SendMsg(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgTest, (resmsg) =>
        {
            Debug.Log("测试回调：" + ((MsgTest)resmsg).RecContent);
        });
    }

    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="registerType"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="code"></param>
    /// <param name="callback"></param>
    public static void Register(RegisterType registerType, string userName, string password, string code, Action<RegisterResult> callback) 
    {
        Debug.Log("ProtocolMgr.Register"+ userName+"  "+ password);
        MsgRegister msg = new MsgRegister();
        msg.RegisterType = registerType;
        msg.Account = userName;
        msg.Password = password;
        msg.Code = code;
        //
        NetManager.Instance.SendMsg(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgRegister, (resmsg) =>
        {
            MsgRegister msgRegister = (MsgRegister)resmsg;
            callback(msgRegister.Result);
        });
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="loginType">登录方式</param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="callback">成功失败</param>
    public static void Login(LoginType loginType,string userName,string password, Action<LoginResult,string> callback) 
    {
        MsgLogin msg = new MsgLogin();
        msg.Account = userName;
        msg.Password = password;
        msg.LoginType = loginType;
        //
        NetManager.Instance.SendMsg(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgLogin,(resmsg)=> 
        {
            MsgLogin msgLogin = (MsgLogin)resmsg;
            callback(msgLogin.Result, msgLogin.Token);
        });
    }
}
