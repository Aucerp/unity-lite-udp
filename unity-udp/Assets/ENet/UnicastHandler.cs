using ENet;
using UnityEngine;

namespace MENet
{
    public class UnicastHandler : IMsgHandler
    {
        private readonly Lobby lobby;

        public UnicastHandler(Lobby lobby)
        {
            this.lobby = lobby;
        }

        public void Handle(string content, Peer sender)
        {
            string[] parts = content.Split(':');
            if (parts.Length < 2) return;

            uint targetPeerID;
            if (uint.TryParse(parts[0], out targetPeerID))
            {
                string message = content.Substring(parts[0].Length + 1);
                MsgSender.SendToPeer("MSG:" + message, sender);
                Debug.Log(string.Format("[UnicastHandler]: 從 Peer {sender.ID} 發送到 Peer {targetPeerID}: {message}"));
            }
        }
    }
} 