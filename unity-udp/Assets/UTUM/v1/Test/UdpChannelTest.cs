using UnityEngine;
using System.Text;
using System.Net;
namespace ver1
{
    public class UdpChannelTest : MonoBehaviour
    {
        private UdpSocketServer server;
        private UdpSocketClient client;

        private void Start()
        {
            // 初始化服務端
            server = new UdpSocketServer(12345);

            // 創建一個通道
            server.CreateChannel("TestChannel");

            server.OnClientConnected = delegate (string channel, EndPoint endpoint)
            {
                Debug.Log("[服務端]: 客戶端已連接到通道 " + channel + " - " + endpoint.ToString());
            };
            server.OnClientDisconnected = delegate (string channel, EndPoint endpoint)
            {
                Debug.Log("[服務端]: 客戶端已從通道 " + channel + " 斷開 - " + endpoint.ToString());
            };
            server.OnMessageReceived = delegate (string channel, byte[] data, EndPoint endpoint)
            {
                string message = Encoding.UTF8.GetString(data);
                Debug.Log("[服務端]: 接收到通道 " + channel + " 的消息: " + message);
                server.Send(channel, Encoding.UTF8.GetBytes("回應: " + message), endpoint);
            };
            server.Start();

            // 初始化客戶端
            client = new UdpSocketClient("127.0.0.1", 12345);
            client.OnConnected = delegate ()
            {
                Debug.Log("[客戶端]: 已連接到 [服務端]");
            };
            client.OnDisconnected = delegate ()
            {
                Debug.Log("[客戶端]: 已從通道斷開");
            };
            client.OnMessageReceived = delegate (byte[] data)
            {
                string message = Encoding.UTF8.GetString(data);
                Debug.Log("[客戶端]: 接收到消息: " + message);
            };

            // 連接到通道並發送測試消息
            client.ConnectToChannel("YOYO");//這會直接觸發 OnConnectFailed 的回調 
            client.SendMessage("Hello from Client!");
        }

        private void OnDestroy()
        {
            if (server != null)
            {
                server.Stop();
            }
            if (client != null)
            {
                client.Close();
            }
        }
    }
}
