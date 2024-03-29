﻿/// <summary>协议类型</summary>
public enum ProtocolEnum
{
    None = 0,
    /// <summary>获取密钥的协议</summary>
    MsgSecret = 1,
    /// <summary>心跳包的协议</summary>
    MsgPing = 2,
    /// <summary>注册</summary>
    MsgRegister = 3,
    /// <summary>登录</summary>
    MsgLogin = 4,
    /// <summary></summary>
    MsgTest = 9999,
}

