using System;
using System.Collections.Generic;
using UnityEngine;
using ENet;

namespace MENet
{
    public class ClientMsgHandlers
    {
        private readonly Dictionary<string, IMsgHandler> handlers;

        public ClientMsgHandlers()
        {
            handlers = new Dictionary<string, IMsgHandler>();
        }

        public void Register(string command, IMsgHandler handler)
        {
            if (!handlers.ContainsKey(command))
            {
                handlers.Add(command, handler);
                Debug.Log("[ClientMsgHandlers]: 註冊處理器 - " + command);
            }
            else
            {
                Debug.LogWarning("[ClientMsgHandlers]: 處理器已存在 - " + command);
            }
        }

        public void Unregister(string command)
        {
            if (handlers.ContainsKey(command))
            {
                handlers.Remove(command);
                Debug.Log("[ClientMsgHandlers]: 移除處理器 - " + command);
            }
            else
            {
                Debug.LogWarning("[ClientMsgHandlers]: 處理器不存在 - " + command);
            }
        }

        public void Handle(string message, Peer peer)
        {
            string[] parts = message.Split(':');
            if (parts.Length < 2)
            {
                Debug.LogWarning("[ClientMsgHandlers]: 無效的消息格式 - " + message);
                return;
            }

            string command = parts[0];
            string content = parts[1];

            if (handlers.ContainsKey(command))
            {
                handlers[command].Handle(content, peer);
            }
            else
            {
                Debug.LogWarning("[ClientMsgHandlers]: 未註冊的消息命令 - " + command);
            }
        }
    }
}
