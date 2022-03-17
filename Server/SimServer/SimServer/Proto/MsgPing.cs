using ProtoBuf;

/// <summary>Ping类</summary>
[ProtoContract]
public class MsgPing : MsgBase
{
    public MsgPing()
    {
        ProtoType = ProtocolEnum.MsgPing;
    }

    [ProtoMember(1)]
    public override ProtocolEnum ProtoType { get; set; }
}