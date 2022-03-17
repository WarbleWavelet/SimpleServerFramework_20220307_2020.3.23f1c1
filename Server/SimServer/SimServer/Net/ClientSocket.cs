using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimServer.Net
{
    /// <summary>客户端</summary>
    public class ClientSocket
    {
        public Socket Socket { get; set; }
        /// <summary>最后服务器Ping客户端的时间</summary>
        public long LastPingTime { get; set; } = 0;

        /// <summary>数据读取</summary>
        public ByteArray ReadBuff = new ByteArray();

        public int UserId = 0;
    }
}
