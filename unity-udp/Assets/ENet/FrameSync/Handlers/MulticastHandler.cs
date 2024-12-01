using ENet;
using UnityEngine;

namespace MENet.FrameSync.Handlers
{
    public class MulticastHandler : IMsgHandler
    {
        private readonly Lobby lobby;

        public MulticastHandler(Lobby lobby)
        {
            this.lobby = lobby;
        }

        public void Handle(string content, Peer sender)
        {
            string[] parts = content.Split(':');
            if (parts.Length < 2) return;

            string channelName = parts[0];
            string message = content.Substring(channelName.Length + 1);
            
            lobby.BroadcastToChannel(channelName, "MSG:" + message);
            Debug.Log("[MulticastHandler]: 從 Peer " + sender.ID + " 發送到頻道 [" + channelName + "]: " + message);
        }
    }
} 