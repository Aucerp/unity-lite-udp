using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
namespace SocketBaseNetwork
{
    public class SimpleSocketClient
    {

        private Socket _clientSocket;
        private bool _isRunning;
        private bool _hasJoined;
        private bool _isClose;

        private MonoBehaviour _monoBehaviour; // 用于启动 Coroutine 的引用

        public delegate void ServerMessageHandler(ServerResponse response, string additionalData);

        public event ServerMessageHandler OnServerMessageReceived;

        public int PlayerID { get; private set; } // 客户端自身的 PlayerID
        public Dictionary<int, IPEndPoint> PlayerList { get; private set; } // 存储所有玩家的列表

        public SimpleSocketClient(MonoBehaviour monoBehaviour, string serverIP, int serverPort, int playerID)
        {
            _monoBehaviour = monoBehaviour;
            PlayerID = playerID;
            PlayerList = new Dictionary<int, IPEndPoint>();

            // 动态分配一个本地端口
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _clientSocket.Bind(new IPEndPoint(IPAddress.Any, 0)); // 绑定到随机端口
            _clientSocket.Connect(new IPEndPoint(IPAddress.Parse(serverIP), serverPort));

            _isRunning = true;
            _isClose = false;

            Debug.Log("[CLIENT INITIALIZED] PlayerID: " + PlayerID + ", Local Port: " +
                      ((IPEndPoint)_clientSocket.LocalEndPoint).Port);

            // 启动 Coroutine
            _monoBehaviour.StartCoroutine(PollSocketCoroutine());
            _monoBehaviour.StartCoroutine(SendJoinRequestsCoroutine());
        }

        private System.Collections.IEnumerator SendJoinRequestsCoroutine()
        {
            while (!_hasJoined && _isRunning)
            {
                SendMessage("JOIN|" + PlayerID);
                Debug.Log("Client: Sent JOIN request with PlayerID: " + PlayerID);

                // 等待 1 秒后继续
                yield return new WaitForSeconds(1f);
            }
        }

        private System.Collections.IEnumerator PollSocketCoroutine()
        {
            while (_isRunning && !_isClose)
            {
                try
                {
                    // 检查是否有数据可读（等待最多1000微秒 = 1毫秒）
                    if (_clientSocket.Poll(1000, SelectMode.SelectRead))
                    {
                        byte[] buffer = new byte[1024];
                        EndPoint serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
                        int receivedBytes = _clientSocket.ReceiveFrom(buffer, ref serverEndpoint);

                        string message = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                        ProcessServerMessage(message);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Client: Error during Poll: " + ex.Message);
                }

                yield return null;
            }
        }

        private void ProcessServerMessage(string message)
        {
            if (message == ServerResponse.JOINED.ToString())
            {
                Debug.Log("Client: Received Join0. Sending Join1...");
                SendMessage("Join1|" + PlayerID); // 回复 Join1
                _hasJoined = true;
            }
            else if (message.StartsWith("CLIENT_LIST|"))
            {
                UpdatePlayerList(message);
                Debug.Log("Client: Acknowledging player list update.");
                SendMessage("LIST_ACK"); // 确认已更新玩家列表
            }
            else
            {
                Debug.LogWarning("Client: Unhandled message: " + message);
            }
        }

        private void UpdatePlayerList(string message)
        {
            try
            {
                string[] parts = message.Replace("CLIENT_LIST|", "").Split(',');
                var newPlayerList = new Dictionary<int, IPEndPoint>();

                foreach (string part in parts)
                {
                    string[] playerData = part.Split(':');
                    if (playerData.Length != 3) continue;

                    int playerID = int.Parse(playerData[0]);
                    string ip = playerData[1];
                    int port = int.Parse(playerData[2]);

                    newPlayerList[playerID] = new IPEndPoint(IPAddress.Parse(ip), port);
                }

                PlayerList = newPlayerList;

                Debug.Log("Client: Player list updated successfully.");
                foreach (var client in PlayerList)
                {
                    Debug.Log(string.Format("Client ID: {0}, IP: {1}, Port: {2}", 
                        client.Key, client.Value.Address, client.Value.Port));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Client: Error updating player list - " + ex.Message);
            }
        }

        public void SendMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            _clientSocket.Send(data);
        }

        public void Close()
        {
            _isClose = true;
            _isRunning = false;
            if (_clientSocket != null)
                _clientSocket.Close();
            Debug.Log("Client: Connection closed.");
        }
    }
}