using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace SimServer.Net
{
    /// <summary>服务器的槽</summary>
    public class ServerSocket : Singleton<ServerSocket>
    {

        #region 字段
        /// <summary>公钥</summary>
        public static string PublicKey = "OceanSever";
        /// <summary>密钥，后续可以随时间进行变化</summary>
        public static string SecretKey = "Ocean_Up&&NB!!";
        /// <summary>协议头大小</summary>
        public static int headLength = 4;

#if DEBUG
        /// <summary>IP地址</summary>
        private string m_IpStr = "127.0.0.1";
#else
        //对应阿里云或腾讯云的 本地ip地址（不是公共ip地址）
        private string m_IpStr = "172.45.756.54";
#endif
        /// <summary>服务器端口</summary>
        private const int m_Port = 8011;
        /// <summary>服务器的监听端口</summary>
        private const int l_Port = 5000;
        /// <summary>心跳包间隔时间</summary>
        public static long m_PingInterval = 30;

        /// <summary>服务器的监听端口</summary>
        private static Socket m_ListenSocket;

        /// <summary>临时保存所有端口（这里多路复用）</summary>
        private static List<Socket> m_CheckReadList = new List<Socket>();

        /// <summary>客户端字典</summary>
        public static Dictionary<Socket, ClientSocket> m_ClientDic = new Dictionary<Socket, ClientSocket>();
        /// <summary>客户端临时列表</summary>
        public static List<ClientSocket> m_TempList = new List<ClientSocket>();
        #endregion

        #region

        public void Init()
        {
            InitServer();
            //
            ExecuteSocket();


        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="msgBase"></param>
        public static void Send(ClientSocket cs, MsgBase msgBase)
        {
            if (cs == null || !cs.Socket. Connected)
            {
                return;
            }

            try
            {
                byte[] sendBytes = ArrayCopy_Send(msgBase);
                //
                try
                {
                    cs.Socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, null, null);
                }
                catch (SocketException ex)
                {
                    Debug.LogError("Socket BeginSend Error：" + ex);
                }
            }
            catch (SocketException ex)
            {
                Debug.LogError("Socket发送数据失败：" + ex);
            }
        }

        #endregion
        #region 辅助2
        /// <summary>处理所有Socket</summary>
        void ExecuteSocket()
        {
            while (true)
            {
                ResetCheckRead();
                //
                try
                {
                    //最后等待时间单位是微秒
                    Socket.Select(m_CheckReadList, null, null, 1000);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                //
                for (int i = m_CheckReadList.Count - 1; i >= 0; i--)
                {
                    Socket s = m_CheckReadList[i];
                    if (s == m_ListenSocket)
                    {
                        ReadListen(s);
                    }
                    else
                    {
                        ReadClient(s);
                    }
                }
                //
                HeartbeatPackageOverTime();
            }
        }
        /// <summary>启动服务器</summary>
        void InitServer()
        {
            IPEndPoint ipEndPoint = GetIPEndPoint(m_IpStr, m_Port);
            SetIPEndPoint(ipEndPoint, l_Port);
            Debug.LogInfo("服务器启动监听{0}成功", m_ListenSocket.LocalEndPoint.ToString());
        }
        /// <summary>处理找出所有socket</summary>
        public void ResetCheckRead()
        {
            m_CheckReadList.Clear();
            m_CheckReadList.Add(m_ListenSocket);
            foreach (Socket s in m_ClientDic.Keys)
            {
                m_CheckReadList.Add(s);
            }
        }
        /// <summary>处理要发送的数据</summary>
        /// <param name="msgBase">数据类</param>
        /// <returns>发送的字节数组</returns>
        static byte[] ArrayCopy_Send(MsgBase msgBase)
        {
            //分为三部分，头：总协议长度；名字；协议内容。
            byte[] nameBytes = MsgBase.EncodeName(msgBase);//协议名字
            byte[] bodyBytes = MsgBase.Encond(msgBase);//协议长度
            int len = nameBytes.Length + bodyBytes.Length;
            byte[] byteHead = BitConverter.GetBytes(len);//发送数据的协议头
            byte[] sendBytes = new byte[byteHead.Length + len];//发送的数据
                                                               //
            Array.Copy(byteHead, 0, sendBytes, 0, byteHead.Length);//协议头
            Array.Copy(nameBytes, 0, sendBytes, byteHead.Length, nameBytes.Length);//协议名字
            Array.Copy(bodyBytes, 0, sendBytes, byteHead.Length + nameBytes.Length, bodyBytes.Length);//协议具体内容

            return sendBytes;

        }
        /// <summary>关闭客户端</summary>
        public void CloseClient(ClientSocket client)
        {
            client.Socket.Close();
            m_ClientDic.Remove(client.Socket);
            Debug.Log("一个客户端断开链接，当前总连接数：{0}", m_ClientDic.Count);
        }
        /// <summary>检测是否心跳包超时的计算</summary>
        void HeartbeatPackageOverTime()
        {
            long timeNow = GetTimeStamp();
            m_TempList.Clear();
            foreach (ClientSocket clientSocket in m_ClientDic.Values)
            {
                if (MoreThanTwoMinutes(timeNow, clientSocket))
                {
                    Debug.Log("Ping Close" + clientSocket.Socket.RemoteEndPoint.ToString());
                    m_TempList.Add(clientSocket);
                }
            }

            foreach (ClientSocket clientSocket in m_TempList)
            {
                CloseClient(clientSocket);
            }
            m_TempList.Clear();
        }

        /// <summary>监听端有客户端链接</summary>
        void ReadListen(Socket listen)
        {
            try
            {
                Socket client = listen.Accept();
                ClientSocket clientSocket = Socket2ClientSocket(client);
                m_ClientDic.Add(client, clientSocket);
                Debug.Log("一个客户端链接：{0},当前{1}个客户端在线！", client.LocalEndPoint.ToString(), m_ClientDic.Count);
            }
            catch (SocketException ex)
            {
                Debug.LogError("Accept fali:" + ex.ToString());
            }
        }
     
        /// <summary>读取客户端</summary>
        void ReadClient(Socket client)
        {
            ClientSocket clientSocket = m_ClientDic[client];
            
            //接受信息，根据信息解析协议，根据协议内容处理消息再下发到客户端
            int count = 0;
            //
            ExpandSizeByReadBuffRemain(clientSocket, out ByteArray readBuff);

            //
            try
            {
                count = client.Receive(readBuff.Bytes, readBuff.WriteIdx, readBuff.Remain, 0);
            }
            catch (SocketException ex)
            {
                Debug.LogError("Receive fali:" + ex);
                CloseClient(clientSocket);
                return;
            }

            //代表客户端断开链接了
            if (count <= 0)
            {
                CloseClient(clientSocket);
                return;
            }

            readBuff.WriteIdx += count;
            
            OnReceiveData(clientSocket);
            readBuff.CheckAndMoveBytes();
        }

        /// <summary>时间戳(-1970110000)</summary>
        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
        /// <summary>转客户端Socket</summary>
        ClientSocket Socket2ClientSocket(Socket client)
        {
            ClientSocket clientSocket = new ClientSocket();
            clientSocket.Socket = client;
            clientSocket.LastPingTime = GetTimeStamp();

            return clientSocket;
        }

        /// <summary>得到IP:端口</summary>
        IPEndPoint GetIPEndPoint(string m_IpStr, int m_Port)
        {
            IPAddress ip = IPAddress.Parse(m_IpStr);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, m_Port);

            return ipEndPoint;
        }

        /// <summary>设置IP:端口</summary>
        void SetIPEndPoint(IPEndPoint ipEndPoint, int listenCount)
        {
            m_ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_ListenSocket.Bind(ipEndPoint);
            m_ListenSocket.Listen(listenCount);

        }
        /// <summary>成功读到协议头</summary> 
        private bool CanReadProtocolHead(ByteArray readbuff, out int bodyLength)
        {
            bodyLength = 0;
            //基本消息长度判断
            if (readbuff.Length <= headLength || readbuff.ReadIdx < 0)
            {
                return false;
            }
            //
            int readIdx = readbuff.ReadIdx;
            byte[] bytes = readbuff.Bytes;
            bodyLength = BitConverter.ToInt32(bytes, readIdx);
            //判断接收到的信息长度  是否小于  包体长度+包体头长度
            //如果小于，代表我们的信息不全；大于代表信息全了（有可能有粘包存在）
            if (readbuff.Length < bodyLength + headLength)
            {
                return false;
            }
            //
            return true;
        }
        /// <summary>解析协议名</summary>
        bool CanReadProtocolName(ClientSocket clientSocket, ByteArray readbuff, out int nameCount, out ProtocolEnum proto)
        {
            nameCount = 0;
            proto = ProtocolEnum.None;
            try
            {
                proto = MsgBase.DecodeName(readbuff.Bytes, readbuff.ReadIdx, out nameCount);//反解析
            }
            catch (Exception ex)
            {
                Debug.LogError("解析协议名出错：" + ex);
                CloseClient(clientSocket);
                return false;
            }

            if (proto == ProtocolEnum.None)
            {
                Debug.LogError("OnReceiveData MsgBase.DecodeName  fail");
                CloseClient(clientSocket);
                return false;
            }

            return true;
        }
        /// <summary>解析协议体</summary>
        bool CanReadProtocolBody(ClientSocket clientSocket, ProtocolEnum proto, ByteArray readbuff, int bodyCount, out MsgBase msgBase)
        {
            msgBase = null;
            try
            {
                msgBase = MsgBase.Decode(proto, readbuff.Bytes, readbuff.ReadIdx, bodyCount);
                if (msgBase == null)
                {
                    Debug.LogError("{0}协议内容解析错误：" + proto.ToString());
                    CloseClient(clientSocket);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("接收数据协议内容解析错误：" + ex);
                CloseClient(clientSocket);
                return false;
            }

            return true;
        }
        /// <summary>还有消息，继续读取数据</summary>

        void ContinueReceiveData(ClientSocket clientSocket, ByteArray readbuff)
        {
            if (readbuff.Length > headLength)
            {
                OnReceiveData(clientSocket);
            }
        }
        /// <summary>通过反射分发消息</summary>
        void DistributeMsg(ClientSocket clientSocket, MsgBase msgBase, ProtocolEnum proto)
        {
            MethodInfo mi = typeof(MsgHandler).GetMethod(proto.ToString());
            object[] o = { clientSocket, msgBase };

            if (mi != null)
            {
                mi.Invoke(null, o);
            }
            else
            {
                Debug.LogError("OnReceiveData Invoke fail:" + proto.ToString());
            }
        }
        /// <summary>
        /// 通过buff判断是否扩容
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="readBuff"></param>
        void ExpandSizeByReadBuffRemain(ClientSocket clientSocket, out ByteArray readBuff)
        {
            readBuff = clientSocket.ReadBuff;
            //如果上一次接收数据刚好占满了1024的数组，
            if (readBuff.Remain <= 0)
            {
                //数据移动到index =0 位置。
                OnReceiveData(clientSocket);
                readBuff.CheckAndMoveBytes();
                //保证到如果数据长度大于默认长度，扩充数据长度，保证信息的正常接收
                while (readBuff.Remain <= 0)
                {
                    int expandSize = readBuff.Length < ByteArray.DEFAULT_SIZE ? ByteArray.DEFAULT_SIZE : readBuff.Length;
                    readBuff.ReSize(expandSize * 2);
                }
            }
        }
        /// <summary>
        /// 解析、接收数据处理
        /// </summary>
        /// 121<param name="clientSocket">客户端socket</param>
        void OnReceiveData(ClientSocket clientSocket)
        {
            ByteArray readbuff = clientSocket.ReadBuff;
            if (CanReadProtocolHead(readbuff, out int bodyLength) == false)
                return;
            //
            readbuff.ReadIdx += headLength;
            if (CanReadProtocolName(clientSocket, readbuff, out int nameCount, out ProtocolEnum proto) == false)
                return;
            //
            readbuff.ReadIdx += nameCount;
            int bodyCount = bodyLength - nameCount;
            MsgBase msgBase = null;
            if (CanReadProtocolBody(clientSocket, proto, readbuff, bodyCount, out msgBase) == false)
                return;
            //
            readbuff.ReadIdx += bodyCount;
            readbuff.CheckAndMoveBytes();
            DistributeMsg(clientSocket, msgBase, proto);
            ContinueReceiveData(clientSocket, readbuff);

        }
        /// <summary>心跳包间隔超过2分钟</summary>
        bool MoreThanTwoMinutes(long timeNow, ClientSocket clientSocket)
        {
            return timeNow - clientSocket.LastPingTime > m_PingInterval * 4;
        }
        #endregion

    }
}
