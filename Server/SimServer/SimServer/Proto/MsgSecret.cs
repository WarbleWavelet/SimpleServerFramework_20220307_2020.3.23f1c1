using ProtoBuf;

/// <summary>加密类</summary>
[ProtoContract]
public class MsgSecret : MsgBase 
{
    //每一个协议类必然包含构造函数来确定当前协议类型，并且都有ProtoType进行序列化标记
    public MsgSecret() 
    {
        ProtoType = ProtocolEnum.MsgSecret;
    }

    
    [ProtoMember(1)]
    public override ProtocolEnum ProtoType { get; set; }

    /// <summary>密钥</summary>
    [ProtoMember(2)]
    public string Srcret;
}