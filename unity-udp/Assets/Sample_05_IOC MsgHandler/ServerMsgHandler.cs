using System;
using System.Collections.Generic;

namespace Sample_05
{
    public class ServerMsgHandler : IMessageHandler
    {
        private readonly Dictionary<string, Action<NetworkMessage>> _messageHandlers;

        public ServerMsgHandler()
        {
            _messageHandlers = new Dictionary<string, Action<NetworkMessage>>();
        }

        public void RegisterHandler(string messageType, Action<NetworkMessage> handler)
        {
            if (!_messageHandlers.ContainsKey(messageType))
            {
                _messageHandlers[messageType] = handler;
            }
        }

        public void HandleMessage(NetworkMessage message)
        {
            if (_messageHandlers.ContainsKey(message.Type))
            {
                Action<NetworkMessage> handler = _messageHandlers[message.Type];
                handler.Invoke(message);
            }
            else
            {
                UnityEngine.Debug.LogWarning("No handler for message type: " + message.Type);
            }
        }
    }
}