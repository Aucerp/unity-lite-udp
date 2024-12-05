//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Threading;
//using Newtonsoft.Json;
//using UnityEngine;

//public class ClientRegistration
//{
//    [JsonProperty("playerID")]
//    public int PlayerID;

//    [JsonProperty("ip")]
//    public string IP;

//    [JsonProperty("port")]
//    public int Port;
//}

//public class ClientList
//{
//    [JsonProperty("clients")]
//    public List<ClientRegistration> Clients;

//    public ClientList()
//    {
//        Clients = new List<ClientRegistration>();
//    }
//}

//public class NetworkMessage
//{
//    public string MessageType { get; set; }
//    public string Data { get; set; }
//}

//public abstract class NetworkManager : IDisposable
//{
//    protected readonly Receiver _receiver;
//    protected readonly Sender _sender;
//    protected readonly IMessageHandler _msgHandler;
//    protected bool _isRunning;
//    private Thread _receiveThread; // 用于存储线程对象

//    protected NetworkManager(int listenPort, string targetAddress, int targetPort, IMessageHandler msgHandler)
//    {
//        if (!NetworkUtils.IsValidIPv4(targetAddress))
//        {
//            throw new System.ArgumentException("Invalid IPv4 address format: " + targetAddress);
//        }

//        _receiver = new Receiver(listenPort);
//        _sender = new Sender(targetAddress, targetPort);
//        _msgHandler = msgHandler;
//        _isRunning = true;
//        StartMessageLoop();
//    }

//    private void StartMessageLoop()
//    {
//        _receiveThread = new Thread(() =>
//        {
//            while (_isRunning)
//            {
//                try
//                {
//                    string message = _receiver.ReceiveMessage();
//                    if (!string.IsNullOrEmpty(message))
//                    {
//                        _receiver.ProcessIncomingData(message);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Debug.LogError(string.Format("Receive thread error: {0}", ex.Message));
//                }

//                Thread.Sleep(10); // 防止高频占用 CPU
//            }
//        });

//        _receiveThread.IsBackground = true; // 设置为后台线程
//        _receiveThread.Start();
//    }

//    public void SendMessage(string messageType, string data)
//    {
//        NetworkMessage message = new NetworkMessage
//        {
//            Type = messageType,
//            Data = data
//        };
//        string jsonMessage = JsonUtility.ToJson(message);
//        _sender.SendMessage(jsonMessage);
//    }

//    public virtual void Dispose()
//    {
//        _isRunning = false; // 停止接收线程
//        if (_receiveThread != null && _receiveThread.IsAlive)
//        {
//            _receiveThread.Join(); // 等待线程安全退出
//        }

//        if (_receiver != null)
//        {
//            _receiver.Close();
//        }

//        if (_sender != null)
//        {
//            _sender.Close();
//        }
//    }
//}

//public class MServer : NetworkManager
//{
//    private Dictionary<int, IPEndPoint> _playerEndPoints = new Dictionary<int, IPEndPoint>();
//    private int _expectedPlayerCount;

//    public MServer(int expectedPlayerCount, string ipAddress, int port)
//        : base(port, ipAddress, 0)
//    {
//        _expectedPlayerCount = expectedPlayerCount;
//        OnMessageReceived += HandleMessage;
//    }

//    private void HandleMessage(string messageType, NetworkMessage message)
//    {
//        if (messageType == "Register")
//        {
//            HandleClientRegistration(message);
//        }
//    }

//    private void HandleClientRegistration(NetworkMessage message)
//    {
//        var registration = JsonConvert.DeserializeObject<ClientRegistration>(message.Data);

//        SendMessage("Echo", message.Data);

//        if (!_playerEndPoints.ContainsKey(registration.PlayerID))
//        {
//            var endpoint = new IPEndPoint(IPAddress.Parse(registration.IP), registration.Port);
//            _playerEndPoints.Add(registration.PlayerID, endpoint);
//            Debug.Log("Player " + registration.PlayerID + " has registered.");

//            if (_playerEndPoints.Count == _expectedPlayerCount)
//            {
//                BroadcastClientList();
//            }
//        }
//    }

//    private void BroadcastClientList()
//    {
//        var clientList = new ClientList();
//        foreach (var kvp in _playerEndPoints.OrderBy(x => x.Key))
//        {
//            clientList.Clients.Add(new ClientRegistration
//            {
//                PlayerID = kvp.Key,
//                IP = kvp.Value.Address.ToString(),
//                Port = kvp.Value.Port
//            });
//        }

//        string json = JsonConvert.SerializeObject(clientList);
//        SendMessage("ClientList", json);
//        Debug.Log("Broadcasted client list.");
//    }
//}

//public class MClient : NetworkManager
//{
//    private int _playerID;
//    private string _localIP;
//    private int _localPort;
//    private bool _isRegistered;
//    private float _registrationInterval;
//    private float _nextRegistrationTime;

//    private Dictionary<int, IPEndPoint> _playerEndPoints = new Dictionary<int, IPEndPoint>();

//    public MClient(int playerID, string serverIP, int serverPort)
//        : base(PortManager.AcquirePort(), serverIP, serverPort)
//    {
//        _playerID = playerID;
//        _localIP = "127.0.0.1";
//        _localPort = Port;
//        _isRegistered = false;
//        _registrationInterval = 1.0f;
//        _nextRegistrationTime = 0;
//        OnMessageReceived += HandleMessage;
//        StartRegistration();
//    }

//    private void HandleMessage(string messageType, NetworkMessage message)
//    {
//        if (messageType == "Echo")
//        {
//            HandleEcho(message);
//        }
//        else if (messageType == "ClientList")
//        {
//            HandleClientList(message);
//        }
//    }

//    private void StartRegistration()
//    {
//        SendRegistration();
//        MonoBehaviour.FindObjectOfType<MonoBehaviour>().StartCoroutine(RegistrationCoroutine());
//    }

//    private IEnumerator RegistrationCoroutine()
//    {
//        while (!_isRegistered)
//        {
//            if (Time.time >= _nextRegistrationTime)
//            {
//                SendRegistration();
//                _nextRegistrationTime = Time.time + _registrationInterval;
//            }
//            yield return null;
//        }
//    }

//    private void SendRegistration()
//    {
//        var registration = new ClientRegistration
//        {
//            PlayerID = _playerID,
//            IP = _localIP,
//            Port = _localPort
//        };

//        string json = JsonConvert.SerializeObject(registration);
//        SendMessage("Register", json);
//    }

//    private void HandleEcho(NetworkMessage message)
//    {
//        var registration = JsonConvert.DeserializeObject<ClientRegistration>(message.Data);
//        if (registration.PlayerID == _playerID)
//        {
//            _isRegistered = true;
//            Debug.Log("Player " + _playerID + " registration confirmed.");
//        }
//    }

//    private void HandleClientList(NetworkMessage message)
//    {
//        var clientList = JsonConvert.DeserializeObject<ClientList>(message.Data);
//        _playerEndPoints.Clear();

//        foreach (var client in clientList.Clients)
//        {
//            var endpoint = new IPEndPoint(IPAddress.Parse(client.IP), client.Port);
//            _playerEndPoints[client.PlayerID] = endpoint;
//        }

//        Debug.Log("Updated local client list with " + _playerEndPoints.Count + " clients.");
//    }

//    public override void Dispose()
//    {
//        base.Dispose();
//        PortManager.ReleasePort(_localPort);
//    }
//}

//public class TestScript : MonoBehaviour
//{
//    private MServer _server;
//    private List<MClient> _clients = new List<MClient>();

//    void Start()
//    {
//        int playerCount = 2;
//        _server = new MServer(playerCount, "127.0.0.1", 23000);

//        for (int i = 1; i <= playerCount; i++)
//        {
//            var client = new MClient(i, "127.0.0.1", 23000);
//            _clients.Add(client);
//        }
//    }

//    void OnDestroy()
//    {
//        if (_server != null)
//        {
//            _server.Dispose();
//        }
//        foreach (var client in _clients)
//        {
//            if (client != null)
//            {
//                client.Dispose();
//            }
//        }
//    }
//}
