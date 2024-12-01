using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using MENet.FrameSync;
using MENet.FrameSync.Handlers;
using System.Text;

namespace MENet
{
    public class ENetManager : MonoBehaviour
    {
        private static readonly bool ENABLE_DEBUG_LOG = false;
        public static ENetManager Instance { get; private set; }
        public int currentFrame { get; private set; }

        [Header("玩家設置")]
        public GameObject playerPrefab;
        public Vector3 spawnPoint = Vector3.zero;
        public int maxPlayers = 5;
        public float playerMoveSpeed = 10f;  // 添加速度設置

        private Dictionary<int, NetworkPlayer> playerObjects = new Dictionary<int, NetworkPlayer>();
        private GameServer _server;
        private List<ENetClient> _clients;

        private bool isRecording = false;
        private bool isGameStarted = false;
        private bool isPlaying = false;
        private bool isPlayingRecord = false;

        private StringBuilder _stringBuilder = new StringBuilder(256);
        private Dictionary<int, List<MoveCommand>> _frameCommandsPool = new Dictionary<int, List<MoveCommand>>();

        private Vector3 _cachedMoveDir = Vector3.zero;
        private Transform _cachedCameraTransform;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private MovementRecorder moveRecorder;
        private const float FRAME_TIME = 1f / 30f;
        private float frameAccumulator = 0;

        private const ushort ServerPort = 12345;
        private string _serverIP = "127.0.0.1";

        // 添加公共方法來訪問玩家信息
        public Dictionary<int, NetworkPlayer> GetPlayerObjects()
        {
            // 返回一個新的字典副本，這樣外部代碼就不能修改原始字典
            return new Dictionary<int, NetworkPlayer>(playerObjects);
        }

        public NetworkPlayer GetPlayerObject(int playerId)
        {
            NetworkPlayer player;
            if (playerObjects.TryGetValue(playerId, out player))
            {
                return player;
            }
            return null;
        }

        public void AddPlayer(int playerId, NetworkPlayer player)
        {
            if (!playerObjects.ContainsKey(playerId))
            {
                playerObjects.Add(playerId, player);
                Debug.Log("[ENetManager] 添加玩家對象 - 玩家:" + playerId);
            }
        }

        public void RemovePlayer(int playerId)
        {
            if (playerObjects.ContainsKey(playerId))
            {
                playerObjects.Remove(playerId);
                Debug.Log("[ENetManager] 移除玩家對象 - 玩家:" + playerId);
            }
        }

        void Start()
        {
            _serverIP = GetLocalIPv4();
            Debug.Log("[ENetManager]: 服務器 IP: " + _serverIP);

            InitializeServer();
            InitializeClients();
            moveRecorder = new MovementRecorder();
            _cachedCameraTransform = Camera.main.transform;
        }

        private void InitializeServer()
        {
            _server = new GameServer(ServerPort, maxPlayers);
            _server.Start();
        }

        private void InitializeClients()
        {
            _clients = new List<ENetClient>();
            for (int i = 0; i < maxPlayers; i++)
            {
                CreateClient(i);
            }
        }

        private void CreateClient(int playerID)
        {
            Debug.Log("[ENetManager]: 創建客戶端 ID: " + playerID);
            var client = new ENetClient();
            
            client.OnDisconnected += () => OnClientDisconnectedFromServer(playerID);
            client.OnMessageReceived += (message) => OnClientMessageReceived(message, playerID);
            client.OnConnectFailed += () => OnClientConnectFailed(playerID);
            client.OnChannelConnectFailed += (channelName) => OnClientChannelConnectFailed(playerID, channelName);

            bool hasTriedChannel = false;
            client.OnConnected += () => 
            {
                OnClientConnectedToServer(playerID);
                if (!hasTriedChannel)
                {
                    hasTriedChannel = true;
                    client.TryConnectChannel("Default");
                }
            };

            client.OnChannelConnected += (channelName) => OnClientConnectedToChannel(playerID, channelName);

            client.Connect(_serverIP, ServerPort);
            _clients.Add(client);
        }

        private void OnClientConnectedToServer(int playerID)
        {
            Debug.Log("[ENetManager]: 客戶端 " + playerID + " 已連接到服務器");
        }

        private void OnClientDisconnectedFromServer(int playerID)
        {
            Debug.Log("[ENetManager]: 客戶端 " + playerID + " 已斷開連接");
        }

        private void OnClientMessageReceived(string message, int playerID)
        {
            Debug.Log("[ENetManager]: 客戶端 " + playerID + " 收到消息: " + message);
        }

        private void OnClientConnectFailed(int playerID)
        {
            Debug.LogError("[ENetManager]: 客戶端 " + playerID + " 連接失敗");
        }

        private void OnClientChannelConnectFailed(int playerID, string channelName)
        {
            Debug.LogError("[ENetManager]: 客戶端 " + playerID + " 連接到頻道 [" + channelName + "] 失敗");
        }

        private void OnClientConnectedToChannel(int playerID, string channelName)
        {
            Debug.Log("[ENetManager]: 客戶端 " + playerID + " 已連接到頻道 " + channelName);
            
            // 在出生點生成玩家
            GameObject playerObj = Instantiate(playerPrefab, spawnPoint, Quaternion.identity);
            NetworkPlayer netPlayer = playerObj.AddComponent<NetworkPlayer>();
            netPlayer.Initialize(playerID, true);
            playerObjects[playerID] = netPlayer;

            // 在服務器上創建玩家狀態
            _server.CreatePlayerState(playerID);

            // 如果是本地玩家（ID為0），在 ENetManager 上添加輸入處理器
            if (playerID == 0)
            {
                PlayerInputHandler inputHandler = gameObject.AddComponent<PlayerInputHandler>();
                inputHandler.Initialize(netPlayer);
            }
        }

        private void OnDestroy()
        {
            if (_server != null)
            {
                _server.Stop();
            }

            if (_clients != null)
            {
                foreach (var client in _clients)
                {
                    client.Disconnect();
                }
                _clients.Clear();
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
            return "127.0.0.1";
        }

        // 公共方法，用於外部發送消息
        public void SendMessageFromClient(int playerID, string message)
        {
            if (playerID >= 0 && playerID < _clients.Count)
            {
                _clients[playerID].SendMessage(message);
            }
        }

        public void BroadcastFromServer(string message)
        {
            if (_server != null)
            {
                _server.BroadcastToAll(message);
            }
        }

        // 單播：從特定客戶端發送消息給特定客戶端
        public void UnicastFromClient(int fromClientID, int toClientID, string message)
        {
            if (fromClientID >= 0 && fromClientID < _clients.Count)
            {
                _clients[fromClientID].SendMessage("UNICAST:" + toClientID + ":" + message);
            }
        }

        // 多播：從客戶端發送消息給指定頻道
        public void MulticastFromClient(int fromClientID, string channelName, string message)
        {
            if (fromClientID >= 0 && fromClientID < _clients.Count)
            {
                _clients[fromClientID].SendMessage("MULTICAST:" + channelName + ":" + message);
            }
        }

        // 廣播：從客戶端發送消息給所有客戶端
        public void BroadcastFromClient(int fromClientID, string message)
        {
            if (fromClientID >= 0 && fromClientID < _clients.Count)
            {
                _clients[fromClientID].SendMessage("BROADCAST:" + message);
            }
        }

        public void StartGame()
        {
            isGameStarted = true;
            isPlaying = true;
            isPlayingRecord = false;
            isRecording = true;
            currentFrame = 0;

            // 開始新的記錄
            if (moveRecorder != null)
            {
                moveRecorder.StartNewRecording();
            }

            // 重置所有玩家位置
            foreach (var playerObj in playerObjects.Values)
            {
                if (playerObj != null)
                {
                    playerObj.transform.position = spawnPoint;
                }
            }

            Debug.Log("[ENetManager] 遊戲開始");
        }

        private float playbackInterval = FRAME_TIME;  // 使用與正常遊戲相同的幀時間
        private float playbackTimer = 0f;
        private Queue<MoveCommand> playbackCommands = new Queue<MoveCommand>();
        private Dictionary<int, List<MoveCommand>> frameCommands = new Dictionary<int, List<MoveCommand>>();

        private void OnMovePlayback(MoveCommand cmd)
        {
            // 按幀號分組存儲命令
            if (!frameCommands.ContainsKey(cmd.Frame))
            {
                frameCommands[cmd.Frame] = new List<MoveCommand>();
            }
            frameCommands[cmd.Frame].Add(cmd);
        }

        void Update()
        {
            if (!isGameStarted) return;

            if (isPlayingRecord)
            {
                // 處理回放邏輯
                playbackTimer += Time.deltaTime;
                if (playbackTimer >= playbackInterval)
                {
                    // 處理當前幀的所有命令
                    if (frameCommands.ContainsKey(currentFrame))
                    {
                        foreach (var cmd in frameCommands[currentFrame])
                        {
                            InputCommand inputCmd = cmd.ToInputCommand();
                            _server.AddInputCommand(inputCmd);
                            
                            Debug.Log("[Movement] 回放移動 - 幀:" + cmd.Frame + 
                                " 位置:" + cmd.Position.ToVector3() + 
                                " 方向:" + cmd.Direction.ToVector3());
                        }
                    }
                    
                    currentFrame++;
                    playbackTimer -= playbackInterval;
                }
            }
            else
            {
                // 正常遊戲邏輯
                // 更新服務器邏輯
                if (_server != null)
                {
                    _server.Update(Time.deltaTime);
                }

                // 更新幀計數
                frameAccumulator += Time.deltaTime;
                while (frameAccumulator >= FRAME_TIME)
                {
                    currentFrame++;
                    frameAccumulator -= FRAME_TIME;
                }

                // 只在記錄模式下記錄移動
                if (isRecording)
                {
                    float horizontal = Input.GetAxis("Horizontal");
                    float vertical = Input.GetAxis("Vertical");
                    
                    if (Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f)
                    {
                        _cachedMoveDir.Set(0, 0, 0);
                        
                        if (vertical != 0)
                            _cachedMoveDir += _cachedCameraTransform.forward * vertical;
                        if (horizontal != 0)
                            _cachedMoveDir += _cachedCameraTransform.right * horizontal;
                        
                        _cachedMoveDir.Normalize();
                        
                        var playerObj = GetPlayerObject(0);
                        if (playerObj != null)
                        {
                            var cmd = MoveCommand.Create(
                                0,
                                currentFrame,
                                playerObj.GetCurrentPosition().ToVector3(),
                                _cachedMoveDir,
                                Input.GetButton("Jump")
                            );
                            moveRecorder.RecordMove(cmd);
                            
                            if (ENABLE_DEBUG_LOG)
                            {
                                Debug.Log(string.Format("[ENetManager] 記錄移動 - 幀:{0} 位置:{1} 方向:{2}",
                                    currentFrame,
                                    playerObj.GetCurrentPosition().ToVector3(),
                                    _cachedMoveDir));
                            }
                        }
                    }
                }
            }
        }

        public ENetClient GetClient(int playerID)
        {
            if (playerID >= 0 && playerID < _clients.Count)
            {
                return _clients[playerID];
            }
            return null;
        }

        // 獲取玩家移動速度
        public float GetPlayerMoveSpeed()
        {
            return playerMoveSpeed;
        }

        // 添加狀態查詢方法
        public bool IsPlaying() { return isPlaying; }
        public bool IsPlayingRecord() { return isPlayingRecord; }

        public void PlayRecord()
        {
            isGameStarted = true;
            isPlaying = false;
            isPlayingRecord = true;
            isRecording = false;
            currentFrame = 0;
            playbackTimer = 0f;
            playbackCommands.Clear();
            frameCommands.Clear();

            Debug.Log("[ENetManager] 開始回放準備 - 重置狀態");

            // 重置所有玩家位置和狀態
            foreach (var playerObj in playerObjects.Values)
            {
                if (playerObj != null)
                {
                    // 重置位置
                    playerObj.transform.position = spawnPoint;
                    
                    // 重置 NetworkPlayer 狀態
                    NetworkPlayer netPlayer = playerObj.GetComponent<NetworkPlayer>();
                    if (netPlayer != null)
                    {
                        FPSPlayerState newState = new FPSPlayerState(netPlayer.PlayerID);
                        netPlayer.UpdateState(newState);
                        Debug.Log("[ENetManager] 重置玩家狀態 - 玩家:" + netPlayer.PlayerID);
                    }
                }
            }

            // 重置服務器的同管理器
            if (_server != null)
            {
                _server.ResetStates();
            }

            // 確保所有記錄都已寫入
            if (moveRecorder != null)
            {
                moveRecorder.PrepareForPlayback();
            }

            // 開始回放
            moveRecorder.PlaybackFrom(0, OnMovePlayback);
            Debug.Log("[ENetManager] 開始回放");
        }
    }
} 