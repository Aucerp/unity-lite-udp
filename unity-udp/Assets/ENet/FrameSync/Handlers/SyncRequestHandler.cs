using ENet;
using UnityEngine;

namespace MENet.FrameSync.Handlers
{
    public class SyncRequestHandler : IMsgHandler
    {
        private readonly FrameSyncManager syncManager;

        public SyncRequestHandler(FrameSyncManager manager)
        {
            syncManager = manager;
        }

        public void Handle(string content, Peer sender)
        {
            // 創建當前遊戲狀態的快照
            GameSnapshot snapshot = syncManager.CreateSnapshot();
            
            // 將快照序列化為字符串
            string snapshotData = SerializeSnapshot(snapshot);
            
            // 發送快照給請求的客戶端
            MsgSender.SendToPeer("SYNC:" + snapshotData, sender);
            Debug.Log("[SyncRequestHandler]: 發送同步數據給客戶端 - Peer ID: " + sender.ID);
        }

        private string SerializeSnapshot(GameSnapshot snapshot)
        {
            // 這裡應該實現快照序列化邏輯
            // 簡單示例：
            return string.Format("{0}:{1}", snapshot.Frame, snapshot.RandomSeed);
        }
    }
} 