using System.Collections.Generic;
using ENet;
using UnityEngine;

namespace MENet
{
    public class Channel
    {
        private string name;
        private HashSet<Peer> clients;

        public Channel(string channelName)
        {
            name = channelName;
            clients = new HashSet<Peer>();
        }

        public void AddClient(Peer peer)
        {
            if (!clients.Contains(peer))
            {
                clients.Add(peer);
                Debug.Log("[Channel " + name + "]: 玩家加入 - ID: " + peer.ID);
            }
        }

        public void RemoveClient(Peer peer)
        {
            if (clients.Contains(peer))
            {
                clients.Remove(peer);
                Debug.Log("[Channel " + name + "]: 玩家離開 - ID: " + peer.ID);
            }
        }

        public void Broadcast(string message)
        {
            foreach (Peer peer in clients)
            {
                if (peer.IsSet)
                {
                    MsgSender.SendToPeer(message, peer);
                }
            }
        }

        public bool HasClient(Peer peer)
        {
            return clients.Contains(peer);
        }

        public int ClientCount
        {
            get { return clients.Count; }
        }

        public string Name
        {
            get { return name; }
        }

        public IEnumerable<Peer> GetClients()
        {
            return clients;
        }
    }
} 