using System.Collections.Generic;
using ENet;

namespace MENet
{
    public class ServerMsgHandlers
    {
        private Dictionary<string, IMsgHandler> handlers;

        public ServerMsgHandlers()
        {
            handlers = new Dictionary<string, IMsgHandler>();
        }

        public void Register(string command, IMsgHandler handler)
        {
            if (!handlers.ContainsKey(command))
            {
                handlers.Add(command, handler);
            }
        }

        public void Handle(string message, Peer sender)
        {
            string[] parts = message.Split(':');
            if (parts.Length >= 1)
            {
                string command = parts[0];
                string content = parts.Length > 1 ? message.Substring(command.Length + 1) : string.Empty;

                IMsgHandler handler;
                if (handlers.TryGetValue(command, out handler))
                {
                    handler.Handle(content, sender);
                }
            }
        }

        public void Unregister(string command)
        {
            if (handlers.ContainsKey(command))
            {
                handlers.Remove(command);
            }
        }
    }
}
