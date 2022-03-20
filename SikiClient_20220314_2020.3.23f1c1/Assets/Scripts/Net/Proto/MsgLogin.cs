using ProtoBuf;


/// <summary>登录</summary> 
[ProtoContract]
public class MsgLogin : MsgBase
{
    //每一个协议类必然包含构造函数来确定当前协议类型，并且都有ProtoType进行序列化标记
    public MsgLogin()
    {
        ProtoType = ProtocolEnum.MsgLogin;
    }

    [ProtoMember(1)]
    public override ProtocolEnum ProtoType { get; set; }

    //
    //客户端向服务器发送的数据
    /// <summary>账户</summary>
    [ProtoMember(2)]
    public string Account;
    [ProtoMember(3)]
    public string Password;
    /// <summary>注册方式</summary>
    [ProtoMember(4)]
    public LoginType LoginType;

    //
    //服务器向客户端返回的数据
    [ProtoMember(5)]
    public LoginResult Result;
    /// <summary>用以自动登录</summary>
    [ProtoMember(6)]
    public string Token;
}