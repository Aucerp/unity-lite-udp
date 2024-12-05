using UnityEngine;
using Sample_05;
using System.Collections.Generic;

public class NetworkTest : MonoBehaviour
{
    [SerializeField] private string _serverIP = "127.0.0.1";
    [SerializeField] private int _serverPort = 23000;
    private string _clientIP = "127.0.0.1";

    private Server _server;
    private List<Client> _clients;
    private float _nextPingTime;
    private const float PING_INTERVAL = 2.0f;

    void Awake()
    {
        _clients = new List<Client>();
        StartServer();
        CreateClients(3);
    }

    void Start()
    {
        int index = 1;
        if (_clients != null && _clients.Count > index)
        {
            _clients[index].SendMessage("Custom", "Test message from client!");
        }
        else
        {
            Debug.LogWarning("No clients available to send message");
        }
    }

    void Update()
    {
        if (Time.time >= _nextPingTime)
        {
            SendPingMessages();
            _nextPingTime = Time.time + PING_INTERVAL;
        }

        // 測試按鍵
        if (Input.GetKeyDown(KeyCode.C))
        {
            CreateClient(); // 按C鍵創建新客戶端
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            RemoveLastClient(); // 按X鍵移除最後一個客戶端
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            SendCustomMessage(); // 按M鍵發送自定義消息
        }
    }

    private void StartServer()
    {
        try
        {
            _server = new Server(_serverIP, _serverPort);
            Debug.Log(string.Format("Server started on {0}:{1}", _serverIP, _serverPort));
        }
        catch (System.ArgumentException ex)
        {
            Debug.LogError(string.Format("Failed to start server: {0}", ex.Message));
        }
    }

    private void CreateClients(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CreateClient();
        }
    }

    private void CreateClient()
    {
        try
        {
            var client = new Client(_clientIP, _serverIP, _serverPort);
            _clients.Add(client);
            Debug.Log(string.Format("Client created on {0}:{1}, connecting to {2}:{3}", _clientIP, client.Port, _serverIP, _serverPort));
        }
        catch (System.ArgumentException ex)
        {
            Debug.LogError(string.Format("Failed to create client: {0}", ex.Message));
        }
    }

    private void RemoveLastClient()
    {
        if (_clients.Count > 0)
        {
            var client = _clients[_clients.Count - 1];
            client.Dispose();
            _clients.RemoveAt(_clients.Count - 1);
            Debug.Log("Client removed");
        }
    }

    private void SendPingMessages()
    {
        foreach (var client in _clients)
        {
            client.SendMessage("Ping", string.Format("Ping from client {0}", client.Port));
        }
    }

    private void SendCustomMessage()
    {
        foreach (var client in _clients)
        {
            client.SendMessage("Custom", string.Format("Custom message from client {0}", client.Port));
        }
    }

    void OnDestroy()
    {
        foreach (var client in _clients)
        {
            client.Dispose();
        }
        _clients.Clear();

        if (_server != null)
        {
            _server.Dispose();
            _server = null;
        }
    }
}
