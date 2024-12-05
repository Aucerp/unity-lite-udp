using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
namespace ver1
{
    public class UdpSocketServer
    {
        private Socket socket;
        private byte[] buffer = new byte[1024];
        private Dictionary<string, List<EndPoint>> channels;

        public Action<string, EndPoint> OnClientConnected;
        public Action<string, EndPoint> OnClientDisconnected;
        public Action<string, byte[], EndPoint> OnMessageReceived;

        public UdpSocketServer(int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            channels = new Dictionary<string, List<EndPoint>>();
        }

        /// <summary>
        /// 創建新的通道
        /// </summary>
        public void CreateChannel(string channelName)
        {
            if (!channels.ContainsKey(channelName))
            {
                channels[channelName] = new List<EndPoint>();
                Debug.Log("[服務端]: 創建了通道 " + channelName);
            }
            else
            {
                Debug.LogWarning("[服務端]: 通道 " + channelName + " 已存在");
            }
        }

        public void Start()
        {
            BeginReceive();
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

                if (message.Contains(":"))
                {
                    string[] parts = message.Split(new char[] { ':' }, 2);
                    string channel = parts[0];
                    string payload = parts[1];

                    if (!channels.ContainsKey(channel))
                    {
                        Debug.LogWarning("[服務端]: 未知通道 " + channel);
                        // 發送未知通道的回應給客戶端
                        string response = "ERROR:UnknownChannel";
                        SendRaw(response, remoteEndpoint);
                        BeginReceive();
                        return;
                    }

                    if (payload == "CONNECT")
                    {
                        if (!channels[channel].Contains(remoteEndpoint))
                        {
                            channels[channel].Add(remoteEndpoint);
                            if (OnClientConnected != null)
                            {
                                OnClientConnected(channel, remoteEndpoint);
                            }
                        }
                    }
                    else if (payload == "DISCONNECT")
                    {
                        if (channels[channel].Remove(remoteEndpoint))
                        {
                            if (OnClientDisconnected != null)
                            {
                                OnClientDisconnected(channel, remoteEndpoint);
                            }
                        }
                    }
                    else
                    {
                        if (OnMessageReceived != null)
                        {
                            OnMessageReceived(channel, Encoding.UTF8.GetBytes(payload), remoteEndpoint);
                        }
                    }
                }

                // 繼續接收
                BeginReceive();
            }
            catch (Exception ex)
            {
#if !UNITY_EDITOR
            Debug.LogError("[服務端錯誤]: " + ex.Message);
#endif
            }
        }

        public void Send(string channel, byte[] data, EndPoint endpoint)
        {
            if (!channels.ContainsKey(channel))
            {
                Debug.LogWarning("[服務端]: 嘗試向未知通道發送消息，通道: " + channel);
                return;
            }

            string message = channel + ":" + Encoding.UTF8.GetString(data);
            byte[] sendData = Encoding.UTF8.GetBytes(message);

            socket.BeginSendTo(sendData, 0, sendData.Length, SocketFlags.None, endpoint, SendCallback, null);
        }

        private void SendRaw(string message, EndPoint endpoint)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, endpoint, SendCallback, null);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                socket.EndSendTo(ar);
            }
            catch (Exception ex)
            {
                Debug.LogError("[服務端錯誤]: " + ex.Message);
            }
        }

        public void Stop()
        {
            socket.Close();
        }
    }
}
