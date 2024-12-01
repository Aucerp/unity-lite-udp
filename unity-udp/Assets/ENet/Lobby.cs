using System.Collections.Generic;
using ENet;
using UnityEngine;

namespace MENet
{
    public class Lobby
    {
        private Dictionary<uint, Peer> allClients;  // 所有在線玩家
        private Dictionary<string, Channel> channels;  // 所有頻道
        private Dictionary<uint, string> clientChannels;  // 玩家所在的頻道

        public Lobby()
        {
            allClients = new Dictionary<uint, Peer>();
            channels = new Dictionary<string, Channel>();
            clientChannels = new Dictionary<uint, string>();
            
            CreateChannel("Default");  // 創建默認頻道
        }

        // 玩家連接時調用
        public void AddClient(Peer peer)
        {
            if (!allClients.ContainsKey(peer.ID))
            {
                allClients.Add(peer.ID, peer);
                Debug.Log("[Lobby]: 玩家連接 - ID: " + peer.ID);
            }
        }

        // 玩家斷開時調用
        public void RemoveClient(Peer peer)
        {
            if (allClients.ContainsKey(peer.ID))
            {
                // 從所在頻道移除
                string channelName;
                if (clientChannels.TryGetValue(peer.ID, out channelName))
                {
                    LeaveChannel(peer);
                }

                allClients.Remove(peer.ID);
                clientChannels.Remove(peer.ID);
                Debug.Log("[Lobby]: 玩家斷開 - ID: " + peer.ID);
            }
        }

        // 創建新頻道
        public Channel CreateChannel(string channelName)
        {
            if (!channels.ContainsKey(channelName))
            {
                Channel channel = new Channel(channelName);
                channels.Add(channelName, channel);
                Debug.Log("[Lobby]: 創建頻道 - " + channelName);
                return channel;
            }
            return channels[channelName];
        }

        // 加入頻道
        public bool JoinChannel(Peer peer, string channelName)
        {
            // 先離開當前頻道
            LeaveChannel(peer);

            // 加入新頻道
            Channel channel = GetOrCreateChannel(channelName);
            if (channel != null)
            {
                channel.AddClient(peer);
                clientChannels[peer.ID] = channelName;
                Debug.Log("[Lobby]: 玩家 " + peer.ID + " 加入頻道 " + channelName);
                return true;
            }
            return false;
        }

        // 離開頻道
        public void LeaveChannel(Peer peer)
        {
            string currentChannel;
            if (clientChannels.TryGetValue(peer.ID, out currentChannel))
            {
                Channel channel = channels[currentChannel];
                channel.RemoveClient(peer);
                clientChannels.Remove(peer.ID);
                Debug.Log("[Lobby]: 玩家 " + peer.ID + " 離開頻道 " + currentChannel);
            }
        }

        // 獲取或創建頻道
        private Channel GetOrCreateChannel(string channelName)
        {
            if (!channels.ContainsKey(channelName))
            {
                return CreateChannel(channelName);
            }
            return channels[channelName];
        }

        // 廣播到指定頻道
        public void BroadcastToChannel(string channelName, string message)
        {
            Channel channel;
            if (channels.TryGetValue(channelName, out channel))
            {
                channel.Broadcast(message);
            }
        }

        // 添加獲取所有客戶端的方法
        public IEnumerable<Peer> GetAllClients()
        {
            return allClients.Values;
        }

        // 獲取指定頻道的所有玩家
        public IEnumerable<Peer> GetChannelClients(string channelName)
        {
            Channel channel;
            if (channels.TryGetValue(channelName, out channel))
            {
                return channel.GetClients();
            }
            return new List<Peer>();  // 如果頻道不存在，返回空列表
        }
    }
}
