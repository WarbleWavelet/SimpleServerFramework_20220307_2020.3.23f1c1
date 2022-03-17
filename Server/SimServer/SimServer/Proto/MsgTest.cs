using ProtoBuf;

/// <summary>Test类</summary>
[ProtoContract]
public class MsgTest : MsgBase
{
    public MsgTest()
    {
        ProtoType = ProtocolEnum.MsgTest;
    }

    [ProtoMember(1)]
    public override ProtocolEnum ProtoType { get; set; }

    [ProtoMember(2)]
    public string ReqContent { get; set; }
    public string RecContent { get; internal set; }

   // [ProtoMember(3)]
}
   