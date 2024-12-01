using ENet;
using UnityEngine;

namespace MENet
{
    public class MsgHandler : IMsgHandler
    {
        public void Handle(string content, Peer sender)
        {
            Debug.Log("[MessageHandler]: 接收到消息 - " + content);
            SendToPeer("ECHO:" + content, sender);
        }

        private void SendToPeer(string message, Peer peer)
        {
            Packet packet = default(Packet);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            packet.Create(data);
            peer.Send(0, ref packet);
        }
    }
}
