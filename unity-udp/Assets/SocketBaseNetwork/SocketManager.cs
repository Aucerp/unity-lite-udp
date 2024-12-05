using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;

namespace SocketBaseNetwork
{
    public class SocketManager : MonoBehaviour
    {
        private SimpleSocketServer _server;
        private List<SimpleSocketClient> _clients; // 存储多个客户端
        private const int ServerPort = 12345; // 固定服务器端口
        private string _serverIP = "127.0.0.1"; // 动态获取的服务器 IP 地址
        public int maxPlayers = 5; // 最大玩家数，设置为 5

        void Start()
        {
            // 动态获取本地 IP 地址
            _serverIP = GetLocalIPv4();
            Debug.Log("Server IP: " + _serverIP);

            // 创建服务器
            _server = new SimpleSocketServer(_serverIP, ServerPort, maxPlayers);
            _server.OnClientJoin += OnClientJoin;

            // 初始化客户端列表
            _clients = new List<SimpleSocketClient>();

            // 创建 5 个客户端
            for (int i = 1; i <= maxPlayers; i++)
            {
                CreateClient(i);
            }
        }

        // 创建客户端的方法
        private void CreateClient(int playerID)
        {
            Debug.Log("Creating client with PlayerID: " + playerID);
            var client = new SimpleSocketClient(this, _serverIP, ServerPort, playerID);
            client.OnServerMessageReceived += (response, additionalData) =>
                OnServerMessageReceived(response, additionalData, playerID);
            _clients.Add(client);
        }

        private void OnClientJoin(int playerID, IPEndPoint clientEndpoint)
        {
            Debug.Log("Server: Client joined - PlayerID: " + playerID + " Endpoint: " + clientEndpoint);
        }

        private void OnServerMessageReceived(ServerResponse response, string additionalData, int playerID)
        {
            Debug.Log("Client " + playerID + " received response: " + response);
            switch (response)
            {
                case ServerResponse.JOINED:
                    Debug.Log("Client " + playerID + " successfully joined the server.");
                    break;

                case ServerResponse.ROOMFULL:
                    Debug.LogError("Client " + playerID + ": Server is full.");
                    break;

                case ServerResponse.PLAYERID_DUPLICATE:
                    Debug.LogError("Client " + playerID + ": PlayerID is duplicate.");
                    break;

                case ServerResponse.UNKNOWN:
                    Debug.LogWarning("Client " + playerID + ": Received unknown message: " + additionalData);
                    break;
            }
        }

        private void OnDestroy()
        {
            // 清理服务器资源
            if (_server != null)
            {
                _server.Close();
            }

            // 清理客户端资源
            if (_clients != null)
            {
                foreach (var client in _clients)
                {
                    client.Close();
                }
            }
        }

        private string GetLocalIPv4()
        {
            foreach (var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return address.ToString();
                }
            }

            throw new System.Exception("No IPv4 address found.");
        }
    }
}