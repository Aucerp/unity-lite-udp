using System;
using System.Net;
using System.Net.Sockets;
namespace Sample_03
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

        public void SendMessage(string message)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
                _socketBroadcaster.SendTo(buffer, _broadcastEndPoint);
                _log.Log("Broadcast sent: " + message);
            }
            catch (Exception ex)
            {
                _log.LogError("Error sending broadcast: " + ex.Message);
            }
            finally
            {
                _socketBroadcaster.Shutdown(SocketShutdown.Both);
                _socketBroadcaster.Close();
            }
        }
    }
}
