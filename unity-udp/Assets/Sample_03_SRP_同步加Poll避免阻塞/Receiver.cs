using System;
using System.Net;
using System.Net.Sockets;
namespace Sample_03//同步poll接收
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

        public string ReceiveMessage()
        {
            byte[] buffer = new byte[512];
            try
            {
                // 非阻塞檢查
                if (_socketReceiver.Poll(1000, SelectMode.SelectRead))//超重要 加了才不會卡住
                {
                    int receivedBytes = _socketReceiver.Receive(buffer);
                    string receivedMessage = System.Text.Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    _log.Log("Received message: " + receivedMessage);
                    return receivedMessage;
                }
                return null; // 無數據時返回 null
            }
            catch (Exception ex)
            {
                _log.LogError("Error receiving broadcast: " + ex.Message);
                return null;
            }
        }

        public void Close()
        {
            _socketReceiver.Close();
            _log.Log("Receiver socket closed.");
            UnityEngine.Debug.Log("Receiver socket closed.");
        }
    }
}
