namespace Sample_05
{
    public class Client : NetworkManager
    {
        private readonly int _clientPort;
        private readonly ClientMessageRegistry _registry;

        public int Port => _clientPort;

        public Client(string localIP, string serverIP, int serverPort) 
            : base(PortManager.AcquirePort(), serverIP, serverPort, new ClientMsgHandler())
        {
            _clientPort = _receiver.Port;
            _registry = new ClientMessageRegistry(_msgHandler as ClientMsgHandler);
            _registry.RegisterHandlers();
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_clientPort > 0)
            {
                PortManager.ReleasePort(_clientPort);
            }
        }
    }
}