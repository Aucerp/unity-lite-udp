using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
namespace Sample_02
{
    // Server: 接收廣播的類
    public class UDPBroadcastReceiver : MonoBehaviour
    {
        private Socket _socketReceiver;
        private IPEndPoint _localEndPoint;

        void Awake()
        {
            // 初始化 Socket
            _socketReceiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _localEndPoint = new IPEndPoint(IPAddress.Any, 23000);

            try
            {
                // 綁定到本地端口
                _socketReceiver.Bind(_localEndPoint);
                Debug.Log("Receiver started, listening for broadcasts...");

                // 啟動協程進行非阻塞接收
                StartCoroutine(ReceiveBroadcastCoroutine());
            }
            catch (Exception ex)
            {
                Debug.LogError("Error initializing receiver: " + ex.Message);
            }
        }

        /// <summary>
        /// 協程：接收廣播消息
        /// </summary>
        private IEnumerator ReceiveBroadcastCoroutine()
        {
            byte[] buffer = new byte[512];

            while (true)
            {
                try
                {
                    // 檢查是否有數據可讀
                    if (_socketReceiver.Poll(1000, SelectMode.SelectRead))
                    {
                        int receivedBytes = _socketReceiver.Receive(buffer);
                        string receivedMessage = System.Text.Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                        // 打印接收到的消息
                        Debug.Log("Received broadcast message: " + receivedMessage);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error receiving broadcast: " + ex.Message);
                }

                // 避免阻塞主線程
                yield return null;
            }
        }

        private void OnDestroy()
        {
            if (_socketReceiver != null)
            {
                _socketReceiver.Close();
                _socketReceiver = null;
            }
        }
    }
}
