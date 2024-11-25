using System;
using System.Net;
using System.Net.Sockets;
namespace Sample_05
{
    public class Sender
    {
        private bool _isClosed;
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

        public void SendMessage(string message)
        {
            if (_isClosed) return;

            try
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
                _socketBroadcaster.SendTo(buffer, _broadcastEndPoint);
                _log.Log("Sender", string.Format("Message sent: {0}", message));
            }
            catch (Exception ex)
            {
                _log.LogError("Sender", "Error sending message", ex);
            }
        }

        public void Close()
        {
            if (!_isClosed)
            {
                _isClosed = true;
                try
                {
                    if (_socketBroadcaster != null)
                    {
                        if (_socketBroadcaster.Connected)
                            _socketBroadcaster.Shutdown(SocketShutdown.Both);
                        _socketBroadcaster.Close();
                    }
                    _log.Log("Sender", "Sender socket closed.");
                }
                catch (Exception ex)
                {
                    _log.LogError("Sender", "Error closing socket", ex);
                }
            }
        }
    }
}