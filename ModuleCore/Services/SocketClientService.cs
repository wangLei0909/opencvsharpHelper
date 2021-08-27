using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ModuleCore.Services
{
    //=================================================================================================================================
    public class SocketClientService
    {
        private TcpClient m_tcpClient;

        public SocketClientService(string ip, int port)
        {
            ServerIP = ip;
            ServerPort = port;
            Task.Run(() => RunTask());
        }

        public string _ip;
        public int _port;

        public string ServerIP
        {
            get { return _ip; }
            set
            {
                _ip = value;
            }
        }

        public int ServerPort
        {
            get { return _port; }
            set
            {
                _port = value;
            }
        }

        private EnumRunFlag m_enumRunFlag;

        public event Action<string> ReceiveString;

        public event Action<string> LogString;

        public int ReceLen { get; private set; }

        /// <summary>
        /// 异步连接请求
        /// </summary>
        async private Task Connect()
        {
            if (m_tcpClient is not null)
                m_tcpClient.Close();
            m_tcpClient = new TcpClient();

            if (m_tcpClient.Connected) return;
            try
            {
                _ = Task.Run(() => LogString?.Invoke($"{DateTime.Now} ;{ServerIP}:{ServerPort}连接中..."));
                await m_tcpClient.ConnectAsync(ServerIP, ServerPort);
                _ = Task.Run(() => LogString?.Invoke($"{DateTime.Now} ;{ServerIP}:{ServerPort}连接成功"));
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                _ = Task.Run(() => LogString?.Invoke($"{ServerIP}:{ServerPort}无法连接...,{ex.Message}"));
                m_enumRunFlag = EnumRunFlag.Sleep;
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public async void SendMsg(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;
            byte[] byteData = Encoding.UTF8.GetBytes(msg);
            if (m_tcpClient != null && m_tcpClient.Connected)
            {
                try
                {
                    await m_tcpClient.GetStream().WriteAsync(byteData.AsMemory(0, byteData.Length));

                    _ = Task.Run(() => LogString?.Invoke($"{DateTime.Now:HH:mm:ss:fff}向{ServerIP}发送信息 =>  " + msg.Replace("\r\n", "")));
                }
                catch (Exception ex)
                {
                    _ = Task.Run(() => LogString?.Invoke("SendMsg Exception: " + ex.Message));

                    m_enumRunFlag = EnumRunFlag.Sleep;
                }
            }
            else
            {
                m_enumRunFlag = EnumRunFlag.Sleep;
            }
        }

        private readonly byte[] m_buffRece = new byte[1024];

        /// <summary>
        /// 接收消息
        /// </summary>
        private async Task ReceMsg()
        {
            if (m_tcpClient != null && m_tcpClient.Connected)
            {
                //下面这个指令会造成m_buffRece数据丢失！
                //Array.Clear(m_buffRece, 0, m_buffRece.Length);

                ReceLen = 0;
                try
                {
                    ReceLen = await m_tcpClient.GetStream().ReadAsync(m_buffRece.AsMemory(0, m_buffRece.Length));
                    // Thread.Sleep(100); 这个没影响
                    //  Task.Delay(100);这个没影响

                    //下面这两种语句会丢数据,因为 await 之后，接管的线程不一定是原来的线程
                    // await Task.Delay(100); 
                    //await Task.Run(() => LogString?.Invoke($"{DateTime.Now} 接收{ServerIP}信息 <= "));  

                    var msg = Encoding.UTF8.GetString(m_buffRece, 0, ReceLen);

                    //服务器异常断开会一直收到 0 长度的信息
                    if (ReceLen == 0)
                    {
                        m_enumRunFlag = EnumRunFlag.Sleep;
                        return;
                    }
                    await Task.Run(() => ReceiveString?.Invoke(msg));

                    //以上两个指令必须一起执行，中间不能有异步操作，否则m_buffRece里的数据会丢失！！！
                    _ = Task.Run(() => LogString?.Invoke($"{DateTime.Now:HH:mm:ss:fff} 接收{ServerIP}信息 <= " + msg));
                }
                catch (Exception ex)
                {
                    _ = Task.Run(() => LogString?.Invoke("ReceMsg Exception: " + ex.Message));

                    m_enumRunFlag = EnumRunFlag.Sleep;
                }
            }
            else
            {
                m_enumRunFlag = EnumRunFlag.Sleep;
            }
        }

        /// <summary>
        /// 异步执行任务
        /// </summary>
        async private void RunTask()
        {
            while (!End)
            {
                switch (m_enumRunFlag)
                {
                    case EnumRunFlag.Init:
                        {
                            await Connect();
                            m_enumRunFlag = EnumRunFlag.Rece;
                            break;
                        }
                    case EnumRunFlag.Rece:
                        {
                            await ReceMsg();
                            break;
                        }

                    case EnumRunFlag.Sleep:
                        {
                            Close();
                            await Task.Delay(3000);
                            m_enumRunFlag = EnumRunFlag.Init;
                            break;
                        }
                }
                await Task.Delay(10);
            }
        }

        /// <summary>
        ///关闭连接
        /// </summary>
        private void Close()
        {
            if (m_tcpClient != null)
            {
                m_tcpClient.Close();
            }
        }

        private bool End = false;

        public void Exit()
        {
            Close();

            End = true;
        }

        public enum EnumRunFlag
        {
            Init,
            Send,
            Rece,
            Sleep
        }
    }
}