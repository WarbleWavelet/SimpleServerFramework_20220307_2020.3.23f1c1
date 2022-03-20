using SimServer.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimServer.Net
{
    /// <summary>根据协议类型选择处理方法</summary> 
    public partial class MsgHandler
    {
        /// <summary>
        /// 所有的协议处理函数都是这个标准，函数名=协议枚举名=类名
        /// </summary>
        /// <param name="c"></param>
        /// <param name="msgBase"></param>
        public static void MsgSecret(ClientSocket c, MsgBase msgBase) 
        {
            //TODO: MsgBase判空

            MsgSecret msgSecret = (MsgSecret)msgBase;
            msgSecret.Srcret = ServerSocket.SecretKey;
            ServerSocket.Send(c, msgSecret);
        }
        /// <summary>
        /// Ping处理
        /// </summary>
        /// <param name="c"></param>
        /// <param name="msgBase"></param>

        public static void MsgPing(ClientSocket c, MsgBase msgBase)
        {
            c.LastPingTime = ServerSocket.GetTimeStamp();//记录服务器给客户端发送的时间
            MsgPing msPong = new MsgPing();
            ServerSocket.Send(c, msPong);
        }
        /// <summary>测试分包</summary>
        public static void MsgTest(ClientSocket c, MsgBase msgBase)
        {
            MsgTest msgTest = (MsgTest)msgBase;
            Debug.Log(msgTest.ReqContent);
            msgTest.RecContent = "服务器发送的数据";//返回值
            ServerSocket.Send(c, msgTest);
            //ServerSocket.Send(c, msgTest);
            //ServerSocket.Send(c, msgTest);
            //ServerSocket.Send(c, msgTest);
            //ServerSocket.Send(c, msgTest);
            //ServerSocket.Send(c, msgTest);
        }












































        /// <summary>
        /// 处理注册信息
        /// </summary>
        /// <param name="c"></param>
        /// <param name="msgBase"></param>
        public static void MsgRegister(ClientSocket c, MsgBase msgBase) 
        {
            MsgRegister msg = (MsgRegister)msgBase;
            Debug.Log("MsgHandler.MsgRegister  "+ msg.Account+"  "+msg.Password);
            var rst = UserManager.Instance.Register(msg.RegisterType, msg.Account, msg.Password, out string token);
            msg.Result = rst;
            ServerSocket.Send(c, msg);
        }

        /// <summary>
        /// 处理登录信息
        /// </summary>
        /// <param name="c"></param>
        /// <param name="msgBase"></param>
        public static void MsgLogin(ClientSocket c, MsgBase msgBase) 
        {
            MsgLogin msg = (MsgLogin)msgBase;
            var rst = UserManager.Instance.Login(msg.LoginType, msg.Account, msg.Password, out int userid, out string token);
            msg.Result = rst;
            msg.Token = token;
            c.UserId = userid;//查表用到
            ServerSocket.Send(c, msg);
        }
    }
}
