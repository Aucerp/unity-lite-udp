using UnityEngine;

namespace Sample_04
{
    public class ReceiverMono : MonoBehaviour
    {
        private Receiver _receiver;

        void Awake()
        {
            _receiver = new Receiver(23000);
            ReceiveMessages();
        }

        private void ReceiveMessages()
        {
            _receiver.ReceiveMessageAsync(message =>
            {
                if (!string.IsNullOrEmpty(message))
                {
                    Debug.Log("Broadcast received: " + message);
                }

                // 繼續接收下一條消息
                ReceiveMessages();
            });
        }

        void OnDestroy()
        {
            if (_receiver != null)
            {
                _receiver.Close();
            }
        }
    }
}
