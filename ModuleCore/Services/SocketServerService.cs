using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModuleCore.Services
{
    public class SocketServerService
    {
        public readonly List<Socket> clientScoketList = new List<Socket>();//存储 连接 服务器 的 客户端
        public readonly List<Socket> badclientScoketList = new List<Socket>();//存储 连接 服务器 的 客户端

        public event Action<string> LogString;         // 连接日志

        public event Action<string> ReceiveResult;

        public SocketServerService(string ip, int port)
        {
            try
            {
                //1、创建Socket对象 参数：寻址方式，当前为Ipv4  指定套接字类型   指定传输协议Tcp；
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //2、绑定端口、IP
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                socket.Bind(iPEndPoint);

                //3、开启侦听   10为队列最多接收的数量
                socket.Listen();
                //如果同时来了100个连接请求，只能处理一个,队列中100个在等待连接的客户端，其他的则返回错误消息。

                //4、开始接受客户端的连接  ，连接会阻塞主线程，故使用线程池。
                ThreadPool.QueueUserWorkItem(new WaitCallback(AcceptClientConnect), socket);
            }
            catch (Exception)
            {
                //    throw;
                Task.Run(() => LogString?.Invoke($"{DateTime.Now} " + "服务端启动失败，检查IP设置或网线！"));
            }
        }

        /// <summary>
        /// 线程池线程执行的接受客户端连接方法
        /// </summary>
        /// <param name="obj">传入的Socket</param>
        private void AcceptClientConnect(object obj)
        {
            //转换Socket
            var serverSocket = obj as Socket;

            _ = Task.Run(() => LogString?.Invoke($"{DateTime.Now} " + "服务端已准备好！"));

            //不断接受客户端的连接
            while (true)
            {
                //5、创建一个负责通信的Socket
                Socket proxSocket = serverSocket.Accept();
                var client = proxSocket.RemoteEndPoint.ToString();
                _ = Task.Run(() => LogString?.Invoke($"{DateTime.Now} " + $"  客户端：{client}  连接上了！"));

                //将连接的Socket存入集合

                clientScoketList.Add(proxSocket); ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveClientMsg), proxSocket);

                //6、不断接收客户端发送来的消息
            }
        }

        /// <summary>
        /// 不断接收客户端信息子线程方法
        /// </summary>
        /// <param name="obj">参数Socke对象</param>
        private void ReceiveClientMsg(object obj)
        {
            Socket proxSocket = obj as Socket;
            var client = proxSocket.RemoteEndPoint.ToString();
            //创建缓存内存，存储接收的信息, 不能放到while中，这块内存可以循环利用
            byte[] data = new byte[1020 * 1024];
            while (true)
            {
                int len;
                try
                {
                    //接收消息,返回字节长度
                    len = proxSocket.Receive(data, 0, data.Length, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    //7、关闭Socket

                    ClientExit($"客户端：{client}非正常退出,异常信息：{ex.Message}", proxSocket);

                    return;//让方法结束，终结当前客户端数据的异步线程，方法退出，即线程结束
                }

                if (len <= 0)//判断接收的字节数   小于0表示正常退出
                {
                    ClientExit($"客户端：{client}正常退出", proxSocket);

                    return;//让方法结束，终结当前客户端数据的异步线程，方法退出，即线程结束
                }
                //将消息显示到TxtLog
                string msgStr = Encoding.UTF8.GetString(data, 0, len);
                if (string.IsNullOrEmpty(msgStr))
                {
                    return;
                }
                //拼接字符串

                _ = Task.Run(() => LogString?.Invoke($"{DateTime.Now:HH:mm:ss:fff} 接收到[{client}]的消息<=：{msgStr.Replace("\r\n", "\\r\\n")}"));
                _ = Task.Run(() => ReceiveResult?.Invoke($"{msgStr}"));
            }
        }

        /// <summary>
        /// 消息广播
        /// </summary>
        public bool SendMsg(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            byte[] data = Encoding.UTF8.GetBytes(str);

            if (clientScoketList.Count > 0)
            {
                _ = Task.Run(() => LogString?.Invoke($"{ DateTime.Now:HH:mm:ss:fff} SendMsg: 向 {clientScoketList.Count} 个客户端发送=>{str}"));
                foreach (var proxSocket in clientScoketList)
                {
                    try
                    {
                        if (proxSocket.Connected)//判断客户端是否还在连接
                        {
                            _ = proxSocket.Send(data, 0, data.Length, SocketFlags.None); //指定套接字的发送行为
                            string msg = proxSocket.RemoteEndPoint.ToString();

                        }
                        else
                        {
                            badclientScoketList.Add(proxSocket);
                        }
                    }
                    catch
                    {
                        badclientScoketList.Add(proxSocket);
                    }
                }

                foreach (var proxSocket in badclientScoketList)
                {
                    if (clientScoketList.Contains(proxSocket))
                    {
                        ClientExit("有问题的客户端", proxSocket);
                    }
                }
                badclientScoketList.Clear();


            }
            else
            {
                _ = Task.Run(() => LogString?.Invoke("SendMsg:没有客户机接入"));
            }

            return true;
        }

        /// <summary>
        /// 向指定客户端发送消息
        /// </summary>
        /// <param name="str"></param>
        /// <param name="proxSocket"></param>
        /// <returns></returns>
        public bool SendMsg(string str, Socket proxSocket)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            if (proxSocket.Connected)//判断客户端是否还在连接
            {
                byte[] data = Encoding.UTF8.GetBytes(str);

                _ = proxSocket.Send(data, 0, data.Length, SocketFlags.None); //指定套接字的发送行为
                string clinet = proxSocket.RemoteEndPoint.ToString();
                _ = Task.Run(() => LogString?.Invoke($"{DateTime.Now:HH:mm:ss:fff} 向连接[{clinet}]发信息=>: " + str.Replace("\r\n", "")));
            }
            else
            {
                _ = Task.Run(() => LogString?.Invoke($"{proxSocket}未连接"));
            }

            return true;
        }

        /// <summary>
        /// 客户端退出调用
        /// </summary>
        /// <param name="msg"></param>
        private void ClientExit(string msg, Socket proxSocket)
        {
            _ = Task.Run(() => LogString?.Invoke(msg));

            try
            {
                _ = clientScoketList.Remove(proxSocket);//移除集合中的连接Socket
                if (proxSocket.Connected)//如果是连接状态
                {
                    proxSocket.Shutdown(SocketShutdown.Both);//关闭连接
                    proxSocket.Close(100);//100秒超时间
                }
            }
            catch (Exception ex)
            {
                _ = Task.Run(() => LogString?.Invoke($"ClientExit 方法异常:{ex.Message}"));
            }
        }
    }

    //=================================================================================================================================
}