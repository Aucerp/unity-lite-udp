using ENet;
using System.Collections.Generic;

namespace MENet
{
    public static class MsgSender
    {
        public static void Broadcast(string message, IEnumerable<Peer> clients)
        {
            foreach (var client in clients)
            {
                SendToPeer(message, client);
            }
        }

        public static void SendToPeer(string message, Peer peer)
        {
            Packet packet = default(Packet);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            packet.Create(data);
            peer.Send(0, ref packet);
        }
    }
}
