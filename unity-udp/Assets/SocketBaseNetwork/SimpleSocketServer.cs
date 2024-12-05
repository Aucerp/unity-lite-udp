using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
namespace SocketBaseNetwork
{
    public enum ServerResponse
    {
        JOINED,
        ROOMFULL,
        PLAYERID_DUPLICATE,
        UNKNOWN,
        CLIENT_LIST
    }

    public class SimpleSocketServer
    {
        private readonly Socket _serverSocket;
        private readonly Thread _listenThread;
        private readonly Dictionary<int, IPEndPoint> _playerMap; // 保存 PlayerID 和 IPEndPoint
        private readonly HashSet<int> _readyClients; // 追踪已准备的客户端
        private readonly int _maxPlayers;
        private int _connectedPlayers = 0; // 当前已连接的玩家数
        private bool _isRunning;

        public Action<int, IPEndPoint> OnClientJoin;

        public SimpleSocketServer(string ipAddress, int port, int playerCount)
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));

            _playerMap = new Dictionary<int, IPEndPoint>();
            _readyClients = new HashSet<int>();
            _maxPlayers = playerCount;
            _isRunning = true;

            _listenThread = new Thread(ListenForClients)
            {
                IsBackground = true
            };
            _listenThread.Start();
        }

        private void ListenForClients()
        {
            byte[] buffer = new byte[1024];
            while (_isRunning)
            {
                try
                {
                    EndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
                    int receivedBytes = _serverSocket.ReceiveFrom(buffer, ref clientEndpoint);
                    string message = Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                    if (message.StartsWith("JOIN|"))
                    {
                        string[] parts = message.Split('|');
                        int playerID = int.Parse(parts[1]);

                        if (_playerMap.ContainsKey(playerID))
                        {
                            SendResponse(ServerResponse.PLAYERID_DUPLICATE, clientEndpoint);
                            continue;
                        }

                        if (_connectedPlayers >= _maxPlayers)
                        {
                            SendResponse(ServerResponse.ROOMFULL, clientEndpoint);
                            continue;
                        }

                        // 添加玩家
                        _connectedPlayers++;
                        _playerMap[playerID] = (IPEndPoint)clientEndpoint;

                        Debug.Log("Server: Player " + playerID + " joined.");
                        SendResponse(ServerResponse.JOINED, clientEndpoint); // 发送 Join0

                        if (OnClientJoin != null)
                            OnClientJoin.Invoke(playerID, (IPEndPoint)clientEndpoint);
                    }
                    else if (message.StartsWith("Join1|"))
                    {
                        string[] parts = message.Split('|');
                        int playerID = int.Parse(parts[1]);

                        if (!_playerMap.ContainsKey(playerID))
                        {
                            Debug.LogWarning("Server: Received Join1 from unknown player ID: " + playerID);
                            continue;
                        }

                        _readyClients.Add(playerID);
                        Debug.Log("Server: Client " + playerID + " is ready.");

                        if (_readyClients.Count == _connectedPlayers)
                        {
                            BroadcastClientIPList();
                        }
                    }
                    else if (message == "LIST_ACK")
                    {
                        Debug.Log("Server: Received LIST_ACK from " + clientEndpoint + ".");
                    }
                }
                catch (Exception ex)
                {
#if !UNITY_EDITOR
                    Debug.LogError("Server Error: " + ex.Message);
#endif
                }
            }
        }

        private void SendResponse(ServerResponse response, EndPoint clientEndpoint)
        {
            string responseMessage = response.ToString();
            _serverSocket.SendTo(Encoding.UTF8.GetBytes(responseMessage), clientEndpoint);
        }

        private void BroadcastClientIPList()
        {
            StringBuilder clientListBuilder = new StringBuilder("CLIENT_LIST|");
            foreach (KeyValuePair<int, IPEndPoint> player in _playerMap)
            {
                clientListBuilder.Append(player.Key).Append(":")
                    .Append(player.Value.Address).Append(":")
                    .Append(player.Value.Port).Append(",");
            }

            string clientList = clientListBuilder.ToString().TrimEnd(',');

            byte[] data = Encoding.UTF8.GetBytes(clientList);
            foreach (IPEndPoint endpoint in _playerMap.Values)
            {
                _serverSocket.SendTo(data, endpoint);
            }

            Debug.Log("Server: Broadcast player list to all clients.");
        }

        public void Close()
        {
            _isRunning = false;
            _serverSocket.Close();
            if (_listenThread.IsAlive)
            {
                _listenThread.Abort();
            }
        }
    }
}