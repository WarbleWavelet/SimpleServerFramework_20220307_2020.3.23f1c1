using ProtoBuf;

/// <summary>注册</summary>
[ProtoContract]
public class MsgRegister : MsgBase
{
    //每一个协议类必然包含构造函数来确定当前协议类型，并且都有ProtoType进行序列化标记
    public MsgRegister()
    {
        ProtoType = ProtocolEnum.MsgRegister;
    }

    [ProtoMember(1)]
    public override ProtocolEnum ProtoType { get; set; }

    //
    //客户端向服务器发送的数据
    [ProtoMember(2)]
    public string Account;
    [ProtoMember(3)]
    public string Password;
    /// <summary>验证码</summary>
    [ProtoMember(4)]
    public string Code;
    [ProtoMember(5)]
    public RegisterType RegisterType;

    //
    //服务器向客户端返回的数据
    [ProtoMember(6)]
    public RegisterResult Result;
}
