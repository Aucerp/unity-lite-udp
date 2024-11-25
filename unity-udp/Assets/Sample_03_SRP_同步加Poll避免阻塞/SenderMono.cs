using UnityEngine;
namespace Sample_03
{
    public class SenderMono : MonoBehaviour
    {
        private Sender _sender;

        void Start()
        {
            _sender = new Sender("255.255.255.255", 23000);
            _sender.SendMessage("Hello, this is a broadcast message!");
        }
    }
}
