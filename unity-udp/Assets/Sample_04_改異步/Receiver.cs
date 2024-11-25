using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sample_04//異步接收
{
    public class Receiver
    {
        private readonly Socket _socketReceiver;
        private readonly ILog _log;

        public Receiver(int port)
        {
            _log = new UnityLog();
            _socketReceiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socketReceiver.Bind(new IPEndPoint(IPAddress.Any, port));
        }

        public void ReceiveMessageAsync(Action<string> callback)
        {
            byte[] buffer = new byte[512];
            try
            {
                _socketReceiver.BeginReceive(
                    buffer,
                    0,
                    buffer.Length,
                    SocketFlags.None,
                    asyncResult =>
                    {
                        try
                        {
                            int receivedBytes = _socketReceiver.EndReceive(asyncResult);
                            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                            _log.Log("Received message: " + receivedMessage);
                            callback(receivedMessage); // 執行回調
                        }
                        catch (Exception ex)
                        {
                            // 在異常时关闭 Socket，确保资源释放
                            this.Close();//添加此行 使編輯器停止時 使socket正確釋放
                            _log.LogError("Error receiving broadcast: " + ex.Message);
                            callback(null); // 回傳 null 表示出錯或無數據
                        }
                    },
                    null
                );
            }
            catch (Exception ex)
            {
                _log.LogError("Error initiating receive: " + ex.Message);
                callback(null); // 回傳 null 表示無法開始接收
            }
        }

        public void Close()
        {
            if (_socketReceiver != null && _socketReceiver.Connected)
            {
                try
                {
                    _socketReceiver.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException ex)
                {
                    // 捕获可能的 Socket 关闭异常，仅记录日志
                    _log.LogError("Error during socket shutdown: " + ex.Message);
                }
                finally
                {
                    _socketReceiver.Close();
                    _log.Log("Receiver socket closed.");
                }
            }
        }
    }
}
