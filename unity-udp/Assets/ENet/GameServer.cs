using ENet;
using System.Collections.Generic;
using UnityEngine;
using MENet.FrameSync;
using MENet.FrameSync.Handlers;

namespace MENet
{
    public class GameServer : BaseServer
    {
        private ServerMsgHandlers msgHandlers;
        private Lobby lobby;
        private FrameSyncManager syncManager;

        public GameServer(ushort port, int maxClients)
        {
            Initialize(port, maxClients);
            InitializeGameLogic();
        }

        private void InitializeGameLogic()
        {
            lobby = new Lobby();
            syncManager = new FrameSyncManager();
            msgHandlers = new ServerMsgHandlers();

            // 註冊遊戲邏輯處理器
            msgHandlers.Register("CONNECT", new ConnectHandler(lobby));
            msgHandlers.Register("FRAME_INPUT", new FrameInputHandler(this));
            msgHandlers.Register("SYNC_REQUEST", new SyncRequestHandler(syncManager));

            // 綁定網絡事件到遊戲邏輯
            OnClientConnected += OnGameClientConnected;
            OnClientDisconnected += OnGameClientDisconnected;
            OnMessageReceived += OnGameMessageReceived;

            Debug.Log("[GameServer]: 遊戲邏輯層初始化完成");
        }

        private void OnGameClientConnected(Peer peer)
        {
            lobby.AddClient(peer);
            lobby.JoinChannel(peer, "Default");
            Debug.Log("[GameServer]: 玩家加入遊戲 - Peer ID: " + peer.ID);
        }

        private void OnGameClientDisconnected(Peer peer)
        {
            lobby.RemoveClient(peer);
            Debug.Log("[GameServer]: 玩家離開遊戲 - Peer ID: " + peer.ID);
        }

        private void OnGameMessageReceived(string message, Peer sender)
        {
            string[] parts = message.Split(':');
            if (parts.Length >= 1)
            {
                string command = parts[0];
                if (command == "JOIN_CHANNEL")
                {
                    string channelName = parts[1];
                    if (lobby.JoinChannel(sender, channelName))
                    {
                        MsgSender.SendToPeer("CHANNEL_CONNECTED:" + channelName, sender);
                        Debug.Log("[GameServer] 玩家 " + sender.ID + " 加入頻道 " + channelName);
                    }
                    else
                    {
                        MsgSender.SendToPeer("CHANNEL_CONNECT_FAILED:" + channelName, sender);
                    }
                }
                else
                {
                    msgHandlers.Handle(message, sender);
                }
            }
        }

        public void Update(float deltaTime)
        {
            syncManager.Update(deltaTime);
            Debug.Log("[GameServer] 更新同步管理器 - 當前幀:" + syncManager.CurrentFrame);
        }

        public void BroadcastToAll(string message)
        {
            // 使用 Lobby 廣播到默認頻道
            lobby.BroadcastToChannel("Default", message);
        }

        public void BroadcastToChannel(string channelName, string message)
        {
            lobby.BroadcastToChannel(channelName, message);
        }

        public void CreatePlayerState(int playerID)
        {
            syncManager.AddPlayerState(playerID);
            Debug.Log("[GameServer] 創建玩家狀態 - 玩家:" + playerID);
        }

        public void AddInputCommand(InputCommand cmd)
        {
            if (syncManager != null)
            {
                syncManager.AddInputCommand(cmd);
                Debug.Log("[GameServer] 添加輸入命令 - 玩家:" + cmd.PlayerID + " 幀:" + cmd.Frame);
            }
        }

        public void ResetStates()
        {
            // 重新初始化同步管理器
            syncManager = new FrameSyncManager();
            syncManager.ResetFrame();
            
            // 只為已加入默認頻道的玩家創建狀態
            foreach (var peer in lobby.GetChannelClients("Default"))
            {
                syncManager.AddPlayerState((int)peer.ID);
                Debug.Log("[GameServer] 重置玩家狀態 - Peer ID: " + peer.ID);
            }
            
            Debug.Log("[GameServer] 重置所有玩家狀態");
        }
    }
} 