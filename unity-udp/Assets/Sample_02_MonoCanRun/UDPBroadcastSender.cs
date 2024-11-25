using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Sample_02
{
	// Client: 發送廣播的類
public class UDPBroadcastSender : MonoBehaviour
{
    private Socket _socketBroadcaster;
    private IPEndPoint _broadcastEndPoint;

    void Start()
    {
        // 初始化 Socket
        _socketBroadcaster = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            EnableBroadcast = true
        };
        _broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 23000);

        // 要傳送的字符串
        string message = "Hello, this is a broadcast message!";
        SendBroadcast(message);
    }

    /// <summary>
    /// 發送廣播消息
    /// </summary>
    /// <param name="message">要發送的字符串</param>
    private void SendBroadcast(string message)
    {
        try
        {
            // 將字符串轉換為 UTF-8 編碼的字節數組
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);

            // 發送數據
            _socketBroadcaster.SendTo(buffer, _broadcastEndPoint);
            Debug.Log("Broadcast sent: " + message);

            // 關閉 Socket
            _socketBroadcaster.Shutdown(SocketShutdown.Both);
            _socketBroadcaster.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error sending broadcast: " + ex.Message);
        }
    }
}
}
