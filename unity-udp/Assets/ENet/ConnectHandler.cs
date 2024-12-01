using ENet;
using UnityEngine;

namespace MENet
{
    public class ConnectHandler : IMsgHandler
    {
        private readonly Lobby lobby;

        public ConnectHandler(Lobby lobby)
        {
            this.lobby = lobby;
        }

        public void Handle(string content, Peer sender)
        {
            string[] parts = content.Split(':');
            if (parts.Length >= 1)
            {
                string channelName = parts[0];
                if (string.IsNullOrEmpty(channelName))
                {
                    channelName = "Default";
                }

                // 使用新的 JoinChannel 方法
                if (lobby.JoinChannel(sender, channelName))
                {
                    // 發送確認消息
                    MsgSender.SendToPeer("CHANNEL_CONNECTED:" + channelName, sender);
                    Debug.Log("[ConnectHandler]: 客戶端加入頻道 " + channelName + " - Peer ID: " + sender.ID);
                }
                else
                {
                    // 加入失敗
                    MsgSender.SendToPeer("CHANNEL_CONNECT_FAILED:" + channelName, sender);
                    Debug.LogWarning("[ConnectHandler]: 客戶端加入頻道失敗 " + channelName + " - Peer ID: " + sender.ID);
                }
            }
        }
    }
}
