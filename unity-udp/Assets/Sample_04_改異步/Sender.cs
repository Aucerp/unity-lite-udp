using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sample_04
{
    public class Sender
    {
        private readonly Socket _socketBroadcaster;
        private readonly IPEndPoint _broadcastEndPoint;
        private readonly ILog _log;

        public Sender(string broadcastAddress, int port)
        {
            _log = new UnityLog();

            _socketBroadcaster = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                EnableBroadcast = true
            };
            _broadcastEndPoint = new IPEndPoint(IPAddress.Parse(broadcastAddress), port);
        }

        public void SendMessageAsync(string message, Action<bool> callback)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            try
            {
                _socketBroadcaster.BeginSendTo(
                    buffer,
                    0,
                    buffer.Length,
                    SocketFlags.None,
                    _broadcastEndPoint,
                    asyncResult =>
                    {
                        try
                        {
                            _socketBroadcaster.EndSendTo(asyncResult);
                            _log.Log("Broadcast sent: " + message);
                            callback(true); // 成功發送
                        }
                        catch (Exception ex)
                        {
                            _log.LogError("Error sending broadcast: " + ex.Message);
                            callback(false); // 發送失敗
                        }
                    },
                    null
                );
            }
            catch (Exception ex)
            {
                _log.LogError("Error initiating send: " + ex.Message);
                callback(false); // 無法開始發送
            }
        }
    }
}
