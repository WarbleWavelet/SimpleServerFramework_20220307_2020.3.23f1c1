using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

/// <summary>处理消息的单例类</summary>
public class NetManager : Singleton<NetManager>
{

    #region 枚举 字段
    /// <summary>连接状态</summary>
    public enum NetEvent
    {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close = 3,
    }
    /// <summary>公钥</summary>
    public string PublicKey = "OceanSever";
    /// <summary>密钥</summary>
    public string SecretKey { get; private set; }
    /// <summary>服务端</summary>
    private Socket m_Socket;
    /// <summary>要读取的buff</summary>
    private ByteArray m_ReadBuff;
    /// <summary>IP</summary>
    private string m_Ip;
    /// <summary>端口</summary>
    private int m_Port;
    //链接状态
    private bool m_Connecting = false;
    private bool m_Closing = false;
    private bool m_Closed = false;
    /// <summary></summary>
    private Thread m_MsgThread;
    /// <summary></summary>
    private Thread m_HeartThread;

    static long lastPingTime;
    /// <summary>最后接受信息时间</summary>
    static long lastPongTime;
    /// <summary>向服务器发送消息的队列</summary>
    private Queue<ByteArray> m_WriteQueue;

    private List<MsgBase> m_MsgList;
    private List<MsgBase> m_UnityMsgList;
    /// <summary>消息列表长度</summary>
    private int m_MsgCount = 0;
    /// <summary>协议头长度</summary>
    static int protocolHeadLength = 4;
    /// <summary>时间间隔的标准，超过它的多少倍就执行</summary>
    public static long m_PingInterval = 30;
    /// <summary>监听连接成功还是失败</summary>
    public delegate void EventListener(string str);
    /// <summary>事件监听的字典</summary> 
    private Dictionary<NetEvent, EventListener> m_ListenerDic = new Dictionary<NetEvent, EventListener>();
    /// <summary>对协议的监听</summary>
    public delegate void ProtoListener(MsgBase msg);
    private Dictionary<ProtocolEnum, ProtoListener> m_ProtoDic = new Dictionary<ProtocolEnum, ProtoListener>();
    /// <summary>掉线</summary>
    private bool m_Diaoxian = false;
    //是否链接成功过（只要链接成功过就是true，再也不会变成false）
    private bool m_IsConnectSuccessed = false;
    private bool m_ReConnect = false;

    private NetworkReachability m_CurNetWork = NetworkReachability.NotReachable;
    #endregion

    #region 辅助1

    /// <summary>在App启动时,获取到当前设备WIFI或者蜂窝有没有开启</summary>
    public IEnumerator CheckNet()
    {
        m_CurNetWork = Application.internetReachability;
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (m_IsConnectSuccessed)
            {
                if (m_CurNetWork != Application.internetReachability)
                {
                    ReConnect();
                    m_CurNetWork = Application.internetReachability;
                }
            }
        }
    }
    /// <summary>放在Mono Update</summary>
    public void Update()
    {
        if (m_Diaoxian && m_IsConnectSuccessed)
        {
            
            ReConnect();
            m_Diaoxian = false;
        }

        //断开链接后，链接服务器之后自动登录
        if (!string.IsNullOrEmpty(SecretKey) && m_Socket.Connected && m_ReConnect)
        {
            LoginByToken();

            m_ReConnect = false;
        }
        MsgUpdate();
    }

    /// <summary>
    /// 链接服务器函数
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public void Connect(string ip, int port)
    {
        if (m_Socket != null && m_Socket.Connected)
        {
            Debug.LogError("链接失败，已经链接了！");
            return;
        }

        if (m_Connecting)
        {
            Debug.LogError("链接失败，正在链接中！");
            return;
        }
        InitState();
        m_Socket.NoDelay = true;
        m_Connecting = true;
        m_Socket.BeginConnect(ip, port, ConnectCallback, m_Socket);
        m_Ip = ip;
        m_Port = port;
    }
    /// <summary>设置密钥</summary>
    public void SetKey(string key)
    {
        SecretKey = key;
    }
    #endregion

    #region 辅助2
    /// <summary>
    /// 重连方法<para/>
    /// 弹框，确定是否重连<para/>
    /// </summary>
    public void ReConnect()
    {
        Connect(m_Ip, m_Port);
        m_ReConnect = true;
    }
    /// <summary>
    /// 使用token登录<para/>
    /// 在本地保存了我们的账户和token，然后进行判断有无账户和token，<para/>
    /// </summary>
    void LoginByToken()
    {
        //ProtocolMgr.Login( LoginType.Token, "username", "token",(res, restoken)=> 
        //{
        //    if (res == LoginResult.Success)
        //    {

        //    }
        //    else 
        //    {

        //    }
        //});
    }
    /// <summary>
    /// 初始化状态
    /// </summary>
    void InitState()
    {
        //初始化变量
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_ReadBuff = new ByteArray();
        m_WriteQueue = new Queue<ByteArray>();
        m_Connecting = false;
        m_Closing = false;
        //
        m_MsgList = new List<MsgBase>();
        m_UnityMsgList = new List<MsgBase>();
        m_MsgCount = 0;
        //还原时间
        lastPingTime = GetTimeStamp();
        lastPongTime = GetTimeStamp();
    }
    /// <summary>
    /// 接受数据回调
    /// </summary>
    /// <param name="ar"></param>
    void ReceiveCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            if (count <= 0)
            {
                Close();
                return;
            }
            //
            m_ReadBuff.WriteIdx += count;
            OnReceiveData();
            if (m_ReadBuff.Remain < 8)
            {
                m_ReadBuff.MoveBytes();
                m_ReadBuff.ReSize(m_ReadBuff.Length * 2);
            }
            //分包：等待下一个包
            socket.BeginReceive(m_ReadBuff.Bytes, m_ReadBuff.WriteIdx, m_ReadBuff.Remain, 0, ReceiveCallBack, socket);
        }
        catch (SocketException ex)
        {
            Debug.LogError("Socket ReceiveCallBack fail:" + ex.ToString());
            Close();
        }
    }
    /// <summary>
    /// 连接回调
    /// </summary>
    /// <param name="ar"></param>
    void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;//服务器上可能不是唯一的socket
            socket.EndConnect(ar);
            ExecuteEvent(NetEvent.ConnectSucc, "");
            m_IsConnectSuccessed = true;
            //
            m_MsgThread = new Thread(ThreadMsg);
            m_MsgThread.IsBackground = true;
            m_MsgThread.Start();
            //
            m_Connecting = false;
            //
            m_HeartThread = new Thread(ThreadPing);//开启心跳线程
            m_HeartThread.IsBackground = true;//保持后台运行
            m_HeartThread.Start();//执行线程
            //
            ProtocolMgr.SecretRequest();
            Debug.Log("Socket Connect Success");
            m_Socket.BeginReceive(m_ReadBuff.Bytes, m_ReadBuff.WriteIdx, m_ReadBuff.Remain, 0, ReceiveCallBack, socket);
        }
        catch (SocketException ex)
        {
            Debug.LogError("Socket Connect fail:" + ex.ToString());
            m_Connecting = false;
        }
    }
    /// <summary></summary>
    ///  /// <param name="normal">正常连接</param>
    void RealClose(bool normal = true)
    {
        SecretKey = "";//密钥为空
        m_Socket.Close();
        ExecuteEvent(NetEvent.Close, normal.ToString());
        m_Diaoxian = true;
        //
        //关闭线程
        if (m_HeartThread != null && m_HeartThread.IsAlive)
        {
            m_HeartThread.Abort();
            m_HeartThread = null;
        }
        if (m_MsgThread != null && m_MsgThread.IsAlive)
        {
            m_MsgThread.Abort();
            m_MsgThread = null;
        }
        Debug.Log("Close Socket");
    }
    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <param name="normal">正常连接</param>
    public void Close(bool normal = true)
    {
        if (m_Socket == null || m_Connecting)
        {
            return;
        }
        //
        if (m_Connecting)
            return;

        //
        if (m_WriteQueue.Count > 0)
        {
            m_Closing = true;
        }
        else
        {
            RealClose(normal);
        }
    }

    /// <summary>
    /// 对数据进行处理（粘包，分包，整包）<para/>
    /// 粘包,多次调用自身惊醒解析客户端解析协议名<para />
    /// </summary>
    void OnReceiveData()
    {
        if (IsCompletedMsg(out int bodyLength) == false)
            return;

        m_ReadBuff.ReadIdx += protocolHeadLength;
        if (CanReadProtocolName(out int nameCount, out ProtocolEnum protocol) == false)
            return;

        m_ReadBuff.ReadIdx += nameCount;
        int bodyCount = bodyLength - nameCount;
        CanReadProtocalBody(protocol, bodyCount);
    }

    /// <summary>
    /// 发送数据到服务器
    /// </summary>
    /// <param name="msgBase"></param>
    public void SendMsg(MsgBase msgBase)
    {
        if (m_Socket == null || !m_Socket.Connected)
        {
            return;
        }

        if (m_Connecting)
        {
            Debug.LogError("正在链接服务器中，无法发送消息！");
            return;
        }

        if (m_Closing && m_Closed == false)
        {
            Debug.LogError("正在关闭链接中，无法发送消息!");
            m_Closed = true;
            return;
        }
        //
        try
        {
            MsgBase2ByteArray(msgBase, out byte[] sendBytes);
            ByteArray ba = new ByteArray(sendBytes);
            int count = 0;
            //
            lock (m_WriteQueue)
            {
                m_WriteQueue.Enqueue(ba);
                count = m_WriteQueue.Count;
            }

            if (count == 1)//因为大于1，m_WriteQueue会自己在回调中处理；回调时又有消息
            {
                m_Socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, m_Socket);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("SendMessage error:" + ex.ToString());
            Close();
        }
    }
    /// <summary>
    /// 数据类转字节数组
    /// </summary>
    /// <param name="msgBase"></param>
    /// <param name="sendBytes"></param>
    /// <param name="ba"></param>
    void MsgBase2ByteArray(MsgBase msgBase, out byte[] sendBytes)
    {
        byte[] nameBytes = MsgBase.EncodeName(msgBase);
        byte[] bodyBytes = MsgBase.Encond(msgBase);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] byteHead = BitConverter.GetBytes(len);
        sendBytes = new byte[byteHead.Length + len];
        Array.Copy(byteHead, 0, sendBytes, 0, byteHead.Length);
        Array.Copy(nameBytes, 0, sendBytes, byteHead.Length, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, byteHead.Length + nameBytes.Length, bodyBytes.Length);
    }
    /// <summary>
    /// 发送结束回调
    /// </summary>
    /// <param name="ar"></param>
    void SendCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            if (socket == null || !socket.Connected)
                return;
            //
            int count = socket.EndSend(ar);
            //判断是否发送完成
            ByteArray ba;
            lock (m_WriteQueue)
            {
                ba = m_WriteQueue.First();
            }
            ba.ReadIdx += count;
            //代表发送完整
            if (ba.Length == 0)
            {
                lock (m_WriteQueue)
                {
                    m_WriteQueue.Dequeue();
                    if (m_WriteQueue.Count > 0)
                    {
                        ba = m_WriteQueue.First();//获取第一数据
                    }
                    else//没有数据
                    {
                        ba = null;
                    }
                }
            }
            //
            //发送不完整或发送完整且存在第二条数据
            if (ba != null)
            {
                socket.BeginSend(ba.Bytes, ba.ReadIdx, ba.Length, 0, SendCallBack, socket);
            }
            //确保关闭链接前，先把消息发送出去
            else if (m_Closing)
            {
                RealClose();
            }
        }
        catch (SocketException ex)
        {
            Debug.LogError("SendCallBack error:" + ex.ToString());
            Close();
        }
    }

    /// <summary>
    /// 监听链接事件
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        if (m_ListenerDic.ContainsKey(netEvent))
        {
            m_ListenerDic[netEvent] += listener;
        }
        else
        {
            m_ListenerDic[netEvent] = listener;
        }
    }
    /// <summary></summary>
    public void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if (m_ListenerDic.ContainsKey(netEvent))
        {
            m_ListenerDic[netEvent] -= listener;
            if (m_ListenerDic[netEvent] == null)
            {
                m_ListenerDic.Remove(netEvent);
            }
        }
    }
    /// <summary>执行事件</summary>
    /// <param name="netEvent"></param>
    /// <param name="str">委托类型</param>
    void ExecuteEvent(NetEvent netEvent, string str)
    {
        if (m_ListenerDic.ContainsKey(netEvent))
        {
            m_ListenerDic[netEvent](str);
        }
    }

    /// <summary>
    /// 一个协议希望只有一个监听
    /// </summary>
    /// <param name="protocolEnum"></param>
    /// <param name="listener"></param>
    public void AddProtoListener(ProtocolEnum protocolEnum, ProtoListener listener)
    {
        m_ProtoDic[protocolEnum] = listener;
    }

    /// <summary>
    /// 执行协议
    /// </summary>
    /// <param name="protocolEnum"></param>
    /// <param name="msgBase"></param>
    public void ExecuteProto(ProtocolEnum protocolEnum, MsgBase msgBase)
    {
        if (m_ProtoDic.ContainsKey(protocolEnum))
        {
            m_ProtoDic[protocolEnum](msgBase);
        }
    }
    /// <summary>服务器的线程</summary>
    void ThreadMsg()
    {

        while (m_Socket != null && m_Socket.Connected)
        {
            if (m_MsgList.Count <= 0) //没有消息
                continue;
            //
            GetMsgBase(m_MsgList, out MsgBase msgBase);


            if (msgBase != null)
            {
                ExecuteMsgBase(msgBase);
            }
            else
            {
                break;
            }
        }
    }

    void ThreadPing()
    {
        while (m_Socket != null && m_Socket.Connected)
        {
            long timeNow = GetTimeStamp();
            if (timeNow - lastPingTime > m_PingInterval)
            {
                MsgPing msgPing = new MsgPing();
                SendMsg(msgPing);
                lastPingTime = GetTimeStamp();
            }

            //现在时间-上一次ping的时间 > 时间间隔，就关闭连接
            int para = 4;//倍数
            if (timeNow - lastPongTime > m_PingInterval * para)
            {
                Close(false);
            }
        }
    }
    void GetMsgBase(List<MsgBase> list,out MsgBase msgBase)
    {
        msgBase = null;
        lock (list)
        {
            if (list.Count > 0)
            {
                msgBase = list[0];
                list.RemoveAt(0);
            }
        }

        //MsgBase msgBase = null;
        //lock (m_MsgList)
        //{
        //    if (m_MsgList.Count > 0)
        //    {
        //        msgBase = m_MsgList[0];
        //        m_MsgList.RemoveAt(0);
        //    }
        //}
        //
    }
    void GetMsgBase(List<MsgBase> list,ref int count,out MsgBase msgBase)
    {
        msgBase = null;
        lock (list)
        {
            if (list.Count > 0)
            {
                msgBase = list[0];
                list.RemoveAt(0);
                count--;
            }
        }
    }
    void ExecuteMsgBase(MsgBase msgBase)
    {
        if (msgBase is MsgPing)
        {
            lastPongTime = GetTimeStamp();
            Debug.Log("客户端收到心跳包！！！！！！！");
            m_MsgCount--;
        }
        else
        {
            lock (m_UnityMsgList)
            {
                m_UnityMsgList.Add(msgBase);
            }
        }
    }
    /// <summary>
    /// 上次ping的时间
    /// </summary>
    /// <returns></returns>
    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }
    /// <summary>
    /// 解析协议名字<para />
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    bool CanReadProtocolName(out int nameCount, out ProtocolEnum protocol)
    {
        nameCount = 0;
        protocol = MsgBase.DecodeName(m_ReadBuff.Bytes, m_ReadBuff.ReadIdx, out nameCount);
        if (protocol == ProtocolEnum.None)
        {
            Debug.LogError("OnReceiveData MsgBase.DecodeName fail");
            Close();
            return false;
        }

        return true;
    }
    /// <summary>包是否完整</summary>
    bool IsCompletedMsg(out int bodyLength)
    {
        bodyLength = 0;
        if (m_ReadBuff.Length <= protocolHeadLength || m_ReadBuff.ReadIdx < 0)
            return false;

        int readIdx = m_ReadBuff.ReadIdx;
        byte[] bytes = m_ReadBuff.Bytes;
        bodyLength = BitConverter.ToInt32(bytes, readIdx);
        //读取协议长度之后进行判断，如果消息长度小于读出来的消息长度，证明是没有一条完整的数据，是分包
        if (m_ReadBuff.Length < bodyLength + protocolHeadLength)
        {
            return false;
        }

        return true;
    }
    /// <summary>
    /// 解析协议体<para />
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    void CanReadProtocalBody(ProtocolEnum protocol, int bodyCount)
    {

        try
        {
            MsgBase msgBase = MsgBase.Decode(protocol, m_ReadBuff.Bytes, m_ReadBuff.ReadIdx, bodyCount);
            if (msgBase == null)
            {
                Debug.LogError("接受数据协议内容解析出错");
                Close();
                return;
            }
            m_ReadBuff.ReadIdx += bodyCount;
            m_ReadBuff.CheckAndMoveBytes();
            //协议具体的操作
            lock (m_MsgList)//多线程
            {
                m_MsgList.Add(msgBase);
            }
            m_MsgCount++;
            //处理粘包
            if (m_ReadBuff.Length > protocolHeadLength)
            {
                OnReceiveData();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Socket OnReceiveData error:" + ex.ToString());
            Close();
        }
    }
    /// <summary>
    /// 放在Mono Update
    /// </summary>
    void MsgUpdate()
    {
        if (m_Socket != null && m_Socket.Connected)
        {
            if (m_MsgCount == 0)
                return;
            //
            // GetMsgBase(m_UnityMsgList, ref m_MsgCount, out MsgBase msgBase);
            MsgBase msgBase = null;
            lock (m_UnityMsgList)
            {
                if (m_UnityMsgList.Count > 0)
                {
                    msgBase = m_UnityMsgList[0];
                    m_UnityMsgList.RemoveAt(0);
                    m_MsgCount--;
                }
            }
            //
            if (msgBase != null)
            {
                ExecuteProto(msgBase.ProtoType, msgBase);
            }
        }
    }
    #endregion
}
