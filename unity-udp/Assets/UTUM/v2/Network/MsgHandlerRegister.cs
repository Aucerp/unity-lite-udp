using System;
using System.Collections.Generic;
namespace s2
{
    public interface IMsgHandler
    {
        void Handle(string channel, byte[] message, string senderRole, System.Net.EndPoint senderEndpoint);
    }
    public class MsgHandlerRegister
    {
        private readonly Dictionary<string, IMsgHandler> handlers;

        public MsgHandlerRegister()
        {
            handlers = new Dictionary<string, IMsgHandler>();
        }

        public void Register(string messageType, IMsgHandler handler)
        {
            if (!handlers.ContainsKey(messageType))
            {
                handlers.Add(messageType, handler);
            }
            else
            {
                throw new Exception("消息處理器已存在，類型: " + messageType);
            }
        }

        public void Handle(string messageType, string channel, byte[] message, string senderRole, System.Net.EndPoint senderEndpoint)
        {
            if (handlers.ContainsKey(messageType))
            {
                handlers[messageType].Handle(channel, message, senderRole, senderEndpoint);
            }
            else
            {
                UnityEngine.Debug.LogWarning("未註冊的消息類型: " + messageType);
            }
        }
    }
}
