using System;

namespace Sample_05
{
    public class ServerMessageRegistry : MessageRegistry
    {
        private readonly Action<NetworkMessage> _handleClientConnect;
        private readonly Action<NetworkMessage> _handlePingMessage;
        private readonly Action<NetworkMessage> _handleCustomMessage;
        private readonly Action<NetworkMessage> _handlePlayeEPIDMessage;

        public ServerMessageRegistry(ServerMsgHandler msgHandler) : base(msgHandler)
        {
            _handleClientConnect = HandleClientConnect;
            _handlePingMessage = HandlePingMessage;
            _handleCustomMessage = HandleCustomMessage;
            _handlePlayeEPIDMessage = HandlePlayeEPIDMessage;
        }

        public override void RegisterHandlers()
        {
            var handler = _msgHandler as ServerMsgHandler;
            handler.RegisterHandler("Connect", _handleClientConnect);
            handler.RegisterHandler("Ping", _handlePingMessage);
            handler.RegisterHandler("Custom", _handleCustomMessage);
            handler.RegisterHandler("PlayeEPID", _handlePlayeEPIDMessage);
        }

        private void HandleClientConnect(NetworkMessage message)
        {
            UnityEngine.Debug.Log(string.Format("Client connected: {0}", message.Data));
        }

        private void HandlePingMessage(NetworkMessage message)
        {
            UnityEngine.Debug.Log(string.Format("Ping from client: {0}", message.Data));
        }

        private void HandleCustomMessage(NetworkMessage message)
        {
            UnityEngine.Debug.Log(string.Format("Custom message: {0}", message.Data));
        }
        private void HandlePlayeEPIDMessage(NetworkMessage message)
        {
            //Log Player EP and PlayerID
        }
    }
}