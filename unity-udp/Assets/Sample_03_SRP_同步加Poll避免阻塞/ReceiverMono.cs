using UnityEngine;
using System.Collections;
namespace Sample_03
{
    public class ReceiverMono : MonoBehaviour
    {
        private Receiver _receiver;

        void Awake()
        {
            _receiver = new Receiver(23000);
            StartCoroutine(ReceiveMessagesCoroutine());
        }

        private IEnumerator ReceiveMessagesCoroutine()
        {
            while (true)
            {
                string message = _receiver.ReceiveMessage();
                if (!string.IsNullOrEmpty(message))
                {
                    Debug.Log("Broadcast received: " + message);
                }
                yield return null; // 避免阻塞主線程
            }
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
