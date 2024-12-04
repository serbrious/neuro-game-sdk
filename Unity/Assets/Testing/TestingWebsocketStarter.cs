using NeuroSdk.Websocket;
using UnityEngine;

namespace NeuroSdk.Testing
{
    internal class TestingWebsocketStarter : MonoBehaviour
    {
        public void Start()
        {
            GetComponent<WebsocketConnection>().StartWs();
        }

        public void OnEnable()
        {
            // Used to show the checkbox in the inspector
        }
    }
}
