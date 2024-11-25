using System;
using UnityEngine;

namespace Sample_05
{
    public class ClientMessageRegistry : MessageRegistry
    {
        private readonly ILog _log;
        private readonly Action<NetworkMessage> _handleWelcomeMessage;
        private readonly Action<NetworkMessage> _handleServerResponse;

        public ClientMessageRegistry(ClientMsgHandler msgHandler) : base(msgHandler)
        {
            _log = new UnityLog();
            _handleWelcomeMessage = HandleWelcomeMessage;
            _handleServerResponse = HandleServerResponse;
        }

        public override void RegisterHandlers()
        {
            var handler = _msgHandler as ClientMsgHandler;
            handler.RegisterHandler("Welcome", _handleWelcomeMessage);
            handler.RegisterHandler("ServerResponse", _handleServerResponse);
        }

        private void HandleWelcomeMessage(NetworkMessage message)
        {
            _log.Log("ClientMessageRegistry", string.Format("Welcome message: {0}", message.Data));
        }

        private void HandleServerResponse(NetworkMessage message)
        {
            _log.Log("ClientMessageRegistry", string.Format("Server response: {0}", message.Data));
        }
    }
}