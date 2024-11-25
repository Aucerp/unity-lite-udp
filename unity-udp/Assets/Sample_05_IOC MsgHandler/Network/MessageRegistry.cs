namespace Sample_05
{
    public abstract class MessageRegistry
    {
        protected readonly IMessageHandler _msgHandler;

        protected MessageRegistry(IMessageHandler msgHandler)
        {
            _msgHandler = msgHandler;
        }

        public abstract void RegisterHandlers();
    }
}