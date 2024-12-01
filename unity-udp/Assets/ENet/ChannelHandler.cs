using ENet;
using UnityEngine;

namespace MENet
{
    public class ChannelHandler : IMsgHandler
    {
        public void Handle(string content, Peer sender)
        {
            string[] parts = content.Split(':');
            if (parts.Length < 2) return;

            string action = parts[0];
            string data = parts[1];

            switch (action)
            {
                case "JOIN":
                    Debug.Log("[ChannelHandler]: 新成員加入頻道 - Peer ID: " + data);
                    break;
                case "LEAVE":
                    Debug.Log("[ChannelHandler]: 成員離開頻道 - Peer ID: " + data);
                    break;
            }
        }
    }
} 