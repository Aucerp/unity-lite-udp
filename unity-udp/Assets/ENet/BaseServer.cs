using ENet;
using System;
using System.Threading;
using UnityEngine;
using Event = ENet.Event;
using EventType = ENet.EventType;

namespace MENet
{
    public abstract class BaseServer
    {
        protected Host server;
        protected Thread serverThread;
        protected bool isRunning;

        public Action<Peer> OnClientConnected;
        public Action<Peer> OnClientDisconnected;
        public Action<string, Peer> OnMessageReceived;

        protected virtual void Initialize(ushort port, int maxClients)
        {
            Library.Initialize();
            server = new Host();
            Address address = new Address();
            address.Port = port;
            server.Create(address, maxClients);
            Debug.Log("[BaseServer]: 網絡層初始化完成");
        }

        public virtual void Start()
        {
            isRunning = true;
            serverThread = new Thread(NetworkLoop)
            {
                IsBackground = true
            };
            serverThread.Start();
            Debug.Log("[BaseServer]: 網絡層啟動成功");
        }

        protected virtual void NetworkLoop()
        {
            while (isRunning)
            {
                Event netEvent;
                bool hasEvent = server.Service(15, out netEvent) > 0;
                
                if (hasEvent)
                {
                    Event eventCopy = netEvent;
                    MainThreadDispatcher.RunOnMainThread(() =>
                    {
                        HandleNetworkEvent(ref eventCopy);
                    });
                }
            }
        }

        protected virtual void HandleNetworkEvent(ref Event netEvent)
        {
            switch (netEvent.Type)
            {
                case EventType.Connect:
                    if (OnClientConnected != null)
                    {
                        OnClientConnected(netEvent.Peer);
                    }
                    break;

                case EventType.Receive:
                    byte[] data = new byte[netEvent.Packet.Length];
                    netEvent.Packet.CopyTo(data);
                    string message = System.Text.Encoding.UTF8.GetString(data);
                    if (OnMessageReceived != null)
                    {
                        OnMessageReceived(message, netEvent.Peer);
                    }
                    netEvent.Packet.Dispose();
                    break;

                case EventType.Disconnect:
                    if (OnClientDisconnected != null)
                    {
                        OnClientDisconnected(netEvent.Peer);
                    }
                    break;
            }
        }

        public virtual void Stop()
        {
            isRunning = false;
            if (serverThread != null && serverThread.IsAlive)
            {
                serverThread.Join();
            }
            server.Dispose();
            Library.Deinitialize();
            Debug.Log("[BaseServer]: 網絡層已停止");
        }

        protected virtual void SendToPeer(string message, Peer peer)
        {
            Packet packet = default(Packet);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            packet.Create(data);
            peer.Send(0, ref packet);
        }
    }
} 