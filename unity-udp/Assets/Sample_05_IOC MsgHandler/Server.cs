namespace Sample_05
{
    public class Server : NetworkManager
    {
        private readonly ServerMessageRegistry _registry;

        public Server(string ipAddress, int port) : base(port, ipAddress, 0, new ServerMsgHandler())
        {
            _registry = new ServerMessageRegistry(_msgHandler as ServerMsgHandler);
            _registry.RegisterHandlers();
        }
    }
}