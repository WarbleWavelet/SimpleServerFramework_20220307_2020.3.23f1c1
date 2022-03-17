using ProtoBuf;
using SimServer.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;





/// <summary>
/// 数据的序列化、反序列化<para />
/// 协议名编码+协议内容编码<para />
/// </summary>
public class MsgBase
{

    #region 字段 属性
    public virtual ProtocolEnum ProtoType { get; set; }
    /// <summary>保存协议名字长度的数据长度</summary>
    static int protocolName_Length = 2;
    #endregion





    #region 协议名的编码、解码
    /// <summary>
    /// 协议名编码<para />
    /// </summary>
    /// <param name="msgBase">数据类</param>
    /// <returns>字节列表</returns>

    public static byte[] EncodeName(MsgBase msgBase)
    {
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.ProtoType.ToString());
        Int16 len = (Int16)nameBytes.Length;

        byte[] bytes = new byte[protocolName_Length + len];
        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);
        //
        Array.Copy(nameBytes, 0, bytes, protocolName_Length, len);
        return bytes;
    }

    /// <summary>
    /// 协议名解码
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static ProtocolEnum DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;
        //
        if (IsMsg_By_ProtocolNameLength_ProtocolLength(bytes,offset) == ProtocolEnum.None)
        {
            return ProtocolEnum.None;
        }
        //
        Int16 len = (Int16)((bytes[offset + 1] << 8) | bytes[offset]);
        count = protocolName_Length + len;
        try
        {
            string name = System.Text.Encoding.UTF8.GetString(bytes, offset + protocolName_Length, len);
            return (ProtocolEnum)System.Enum.Parse(typeof(ProtocolEnum), name);
        }
        catch (Exception ex)
        {
            Debug.LogError("不存在的协议:" + ex.ToString());
            return ProtocolEnum.None;
        }
    }

    #endregion

    #region 协议内容的编码、解码
    /// <summary>
    /// 协议序列化及加密
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns>加密的字节数组</returns>
    public static byte[] Encond(MsgBase msgBase)
    {
        using (var memory = new MemoryStream())
        {
            //将我们的协议类进行序列化，转换成数组
            Serializer.Serialize(memory, msgBase);
            byte[] bytes = memory.ToArray();
            string secret = Encrypt(msgBase);

            bytes = AES.AESEncrypt(bytes, secret);

            return bytes;
        }
    }

    /// <summary>
    /// 协议解密
    /// </summary>
    /// <param name="protocol"></param>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns>MsgBase类数据</returns>
    public static MsgBase Decode(ProtocolEnum protocol, byte[] bytes, int offset, int count)
    {
        if (count <= 0)
        {
            Debug.LogError("协议解密出错，数据长度为0");
            return null;
        }
        //
        try
        {
            byte[] newBytes = new byte[count];
            //解密
            Array.Copy(bytes, offset, newBytes, 0, count);
            string secret = Encrypt(protocol);
            newBytes = AES.AESDecrypt(newBytes, secret);
            //反序列化
            using (var memory = new MemoryStream(newBytes, 0, newBytes.Length))
            {
                Type t = System.Type.GetType(protocol.ToString());
                return (MsgBase)Serializer.NonGeneric.Deserialize(t, memory);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("协议解密出错:" + ex.ToString());
            return null;
        }
    }
    #endregion


    #region 辅助2
    /// <summary>根据参数判断是密钥还是公钥</summary>
    /// <returns>字符串类型钥</returns>
    static string Encrypt(MsgBase msgBase)
    {
        string secret = ServerSocket.SecretKey;//默认密钥

        if (msgBase is MsgSecret)//本身密钥协议
        {
            secret = ServerSocket.PublicKey;
        }

        return secret;
    }
    /// <summary>根据参数判断是密钥还是公钥</summary>
    /// <returns>字符串类型钥</returns>
    static string Encrypt(ProtocolEnum protocol)
    {
        string secret = ServerSocket.SecretKey;
        if (protocol == ProtocolEnum.MsgSecret)
        {
            secret = ServerSocket.PublicKey;
        }

        return secret;
    }
    /// <summary>通过协议名、协议内容，的数据长度判断是否可读</summary> 
    static ProtocolEnum IsMsg_By_ProtocolNameLength_ProtocolLength(byte[] bytes, int offset)
    {
        if (offset + protocolName_Length > bytes.Length)
            return ProtocolEnum.None;
        //
        Int16 len = (Int16)((bytes[offset + 1] << 8) | bytes[offset]);
        if (offset + protocolName_Length + len > bytes.Length)
            return ProtocolEnum.None;

        return ProtocolEnum.MsgTest;
    }
    #endregion

}

