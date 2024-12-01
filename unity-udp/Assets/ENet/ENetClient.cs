using ENet;
using System;
using System.Text;
using UnityEngine;
using Event = ENet.Event;
using EventType = ENet.EventType;
using System.Threading;

namespace MENet
{
    public class ENetClient
    {
        private Host client;
        private Peer serverPeer;
        private bool isRunning;
        private Thread clientThread;

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnMessageReceived;
        public event Action OnConnectFailed;
        public event Action<string> OnChannelConnectFailed;
        public event Action<string> OnChannelConnected;

        private string currentChannel;

        public ENetClient()
        {
            Library.Initialize();
            client = new Host();
            Debug.Log("[ENetClient]: 初始化客户端");
        }

        public void Connect(string serverAddress, ushort serverPort)
        {
            try
            {
                Address address = new Address();
                address.SetHost(serverAddress);
                address.Port = serverPort;

                client.Create();
                serverPeer = client.Connect(address);
                
                // 啟動事件循環
                isRunning = true;
                clientThread = new Thread(ClientLoop)
                {
                    IsBackground = true
                };
                clientThread.Start();

                Debug.Log("[ENetClient] 嘗試連接到服務器 地址: " + serverAddress + " 端口: " + serverPort);
            }
            catch (Exception e)
            {
                Debug.LogError("[ENetClient]: 連接失敗 - " + e.Message);
                if (OnConnectFailed != null)
                {
                    MainThreadDispatcher.RunOnMainThread(() =>
                    {
                        OnConnectFailed();
                    });
                }
            }
        }

        private void ClientLoop()
        {
            while (isRunning)
            {
                bool polled = false;
                Event netEvent;

                while (!polled)
                {
                    if (client.Service(15, out netEvent) <= 0)
                    {
                        break;
                    }

                    polled = true;

                    HandleEvent(ref netEvent);
                }
            }
        }

        private void HandleEvent(ref Event netEvent)
        {
            switch (netEvent.Type)
            {
                case EventType.Connect:
                    isRunning = true;
                    MainThreadDispatcher.RunOnMainThread(() =>
                    {
                        if (OnConnected != null) OnConnected();
                    });
                    break;

                case EventType.Receive:
                    byte[] data = new byte[netEvent.Packet.Length];
                    netEvent.Packet.CopyTo(data);
                    string message = System.Text.Encoding.UTF8.GetString(data);
                    
                    // 處理頻道連接響應
                    if (message.StartsWith("CHANNEL_CONNECTED:"))
                    {
                        string channelName = message.Substring("CHANNEL_CONNECTED:".Length);
                        MainThreadDispatcher.RunOnMainThread(() =>
                        {
                            if (OnChannelConnected != null) OnChannelConnected(channelName);
                        });
                    }
                    else
                    {
                        MainThreadDispatcher.RunOnMainThread(() =>
                        {
                            if (OnMessageReceived != null) OnMessageReceived(message);
                        });
                    }
                    netEvent.Packet.Dispose();
                    break;

                case EventType.Disconnect:
                    MainThreadDispatcher.RunOnMainThread(() =>
                    {
                        Debug.Log("[ENetClient]: 與服務器斷開連接");
                        if (OnDisconnected != null) OnDisconnected();
                    });
                    break;
            }
        }

        public void Disconnect()
        {
            isRunning = false;
            
            if (clientThread != null && clientThread.IsAlive)
            {
                clientThread.Join();
            }

            if (serverPeer.IsSet)
            {
                serverPeer.DisconnectNow(0);
            }

            client.Flush();
            client.Dispose();
            Debug.Log("[ENetClient]: 客戶端已斷開連接");
        }

        public void SendMessage(string message)
        {
            if (serverPeer.IsSet)
            {
                Packet packet = default(Packet);
                byte[] data = Encoding.UTF8.GetBytes(message);
                packet.Create(data);
                serverPeer.Send(0, ref packet);
                //Debug.Log("[ENetClient]: 發送消息 - " + message);
            }
            else
            {
                Debug.LogWarning("[ENetClient]: 未連接到服務器，無法發送消息");
            }
        }

        internal Peer GetServerPeer()
        {
            return serverPeer;
        }

        public void TryConnectChannel(string channelName)
        {
            if (isRunning)
            {
                SendMessage("JOIN_CHANNEL:" + channelName);
            }
        }
    }
}
