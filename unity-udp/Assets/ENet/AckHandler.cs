using ENet;
using UnityEngine;

namespace MENet
{
    public class AckHandler : IMsgHandler
    {
        public void Handle(string content, Peer sender)
        {
            Debug.Log("[AckHandler]: 收到服務器確認 - " + content);
        }
    }
} 