using UnityEngine;

namespace Sample_04
{
    public class SenderMono : MonoBehaviour
    {
        private Sender _sender;

        void Start()
        {
            _sender = new Sender("255.255.255.255", 23000);
            _sender.SendMessageAsync("Hello, this is a broadcast message!", success =>
            {
                if (success)
                {
                    Debug.Log("Broadcast sent successfully!");
                }
                else
                {
                    Debug.LogError("Failed to send broadcast.");
                }
            });
        }
    }
}
