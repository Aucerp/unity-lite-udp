using ENet;

namespace MENet
{
    public interface IMsgHandler
    {
        void Handle(string content, Peer sender);
    }
}
