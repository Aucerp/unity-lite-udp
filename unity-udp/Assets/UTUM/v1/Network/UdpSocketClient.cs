using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
namespace ver1
{
    public class UdpSocketClient
    {
        private Socket socket;
        private EndPoint serverEndpoint;
        private byte[] buffer = new byte[1024];
        private string currentChannel;

        public Action OnConnected;
        public Action OnDisconnected;
        public Action<byte[]> OnMessageReceived;

        public UdpSocketClient(string serverAddress, int serverPort)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            serverEndpoint = new IPEndPoint(IPAddress.Parse(serverAddress), serverPort);
        }

        public void ConnectToChannel(string channel)
        {
            currentChannel = channel;
            Send(channel + ":CONNECT");
            if (OnConnected != null)
            {
                OnConnected();
            }
            BeginReceive();
        }

        public void DisconnectFromChannel()
        {
            if (currentChannel != null)
            {
                Send(currentChannel + ":DISCONNECT");
                if (OnDisconnected != null)
                {
                    OnDisconnected();
                }
                currentChannel = null;
            }
        }

        public void SendMessage(string message)
        {
            if (currentChannel != null)
            {
                Send(currentChannel + ":" + message);
            }
        }

        private void Send(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, serverEndpoint, SendCallback, null);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                socket.EndSendTo(ar);
            }
            catch (Exception ex)
            {
                Debug.LogError("[客戶端錯誤]: " + ex.Message);
            }
        }

        private void BeginReceive()
        {
            EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remoteEndpoint, ReceiveCallback, remoteEndpoint);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                EndPoint remoteEndpoint = (EndPoint)ar.AsyncState;
                int receivedLength = socket.EndReceiveFrom(ar, ref remoteEndpoint);

                byte[] data = new byte[receivedLength];
                Array.Copy(buffer, data, receivedLength);

                string message = Encoding.UTF8.GetString(data);
                string[] parts = message.Split(new char[] { ':' }, 2);

                if (parts.Length == 2)
                {
                    if (parts[0] == "ERROR" && parts[1] == "UnknownChannel")
                    {
                        Debug.LogError("[客戶端]: 嘗試連接的通道不存在");
                        if(OnDisconnected != null)
                        OnDisconnected.Invoke();
                    }
                    else if (parts[0] == currentChannel)
                    {
                        if (OnMessageReceived != null)
                            OnMessageReceived.Invoke(Encoding.UTF8.GetBytes(parts[1]));
                    }
                }

                // 繼續接收
                BeginReceive();
            }
            catch (Exception ex)
            {
                Debug.LogError("[客戶端錯誤]: " + ex.Message);
            }
        }

        public void Close()
        {
            DisconnectFromChannel();
            socket.Close();
        }
    }
}
