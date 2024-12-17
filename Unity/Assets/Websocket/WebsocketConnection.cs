#nullable enable

using System;
using System.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using NativeWebSocket;
using NeuroSdk.Messages.API;
using NeuroSdk.Utilities;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace NeuroSdk.Websocket
{
    [PublicAPI]
    public sealed class WebsocketConnection : MonoBehaviour
    {
        public static WebsocketConnection? Instance { get; private set; }

        private static WebSocket? _socket;

        public string game = null!;
        public MessageQueue messageQueue = null!;
        public CommandHandler commandHandler = null!;

        private void Awake()
        {
            if (Instance)
            {
                Debug.Log("Destroying duplicate WebsocketConnection instance");
                Destroy(this);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        private void Start() => StartWs().Forget();

        private async UniTask Reconnect()
        {
            await UniTask.SwitchToMainThread();
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            await StartWs();
        }

        private async UniTask StartWs()
        {
            Debug.LogWarning("Initializing WebSocket connection");

            try
            {
                if (_socket?.State is WebSocketState.Open or WebSocketState.Connecting) await _socket.Close();
            }
            catch
            {
                // ignored
            }

            string? websocketUrl = null;

            if (Application.absoluteURL.IndexOf("?", StringComparison.Ordinal) != -1)
            {
                string[] urlSplits = Application.absoluteURL.Split('?');
                if (urlSplits.Length > 1)
                {
                    string[] urlParamSplits = urlSplits[1].Split("WebSocketURL=");
                    if (urlParamSplits.Length > 1)
                    {
                        string? param = urlParamSplits[1].Split('&')[0];
                        if (!string.IsNullOrEmpty(param))
                        {
                            websocketUrl = param;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(websocketUrl))
            {
                try
                {
                    Uri uri = new(Application.absoluteURL);
                    string requestUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}/$env/NEURO_SDK_WS_URL";
                    UnityWebRequest request = UnityWebRequest.Get(requestUrl);

                    await request.SendWebRequest();
                    if (request.result == UnityWebRequest.Result.Success) websocketUrl = request.downloadHandler.text;
                }
                catch
                {
                    // ignored
                }
            }

            if (string.IsNullOrEmpty(websocketUrl))
            {
                websocketUrl = Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.Process) ??
                               Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.User) ??
                               Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.Machine);
            }

            if (string.IsNullOrEmpty(websocketUrl))
            {
                string errMessage = "Could not retrieve websocket URL.";
#if UNITY_EDITOR || !UNITY_WEBGL
                errMessage += " You should set the NEURO_SDK_WS_URL environment variable.";
#endif
#if UNITY_WEBGL
                errMessage += " You need to specify a WebSocketURL query parameter in the URL or open a local server that serves the NEURO_SDK_WS_URL environment variable. See the documentation for more information.";
#endif
                Debug.LogError(errMessage);
                return;
            }

            // Websocket callbacks get run on separate threads! Watch out
            _socket = new WebSocket(websocketUrl);
            _socket.OnMessage += (bytes) =>
            {
                string message = Encoding.UTF8.GetString(bytes);
                ReceiveMessage(message).Forget();
            };
            _socket.OnError += (error) =>
            {
                Debug.LogError("Websocket connection has encountered an error!");
                Debug.LogError(error);
                Reconnect().Forget();
            };
            _socket.OnClose += (_) =>
            {
                Debug.LogError("Websocket connection has been closed!");
                Reconnect().Forget();
            };
            await _socket.Connect();
        }

        private void Update()
        {
            if (_socket?.State is not WebSocketState.Open) return;

            while (messageQueue.Count > 0)
            {
                OutgoingMessageBuilder builder = messageQueue.Dequeue()!;
                SendTask(builder).Forget();
            }

#if !UNITY_WEBGL || UNITY_EDITOR
            _socket.DispatchMessageQueue();
#endif
        }

        private async UniTask SendTask(OutgoingMessageBuilder builder)
        {
            string message = Jason.Serialize(builder.GetWsMessage());

            Debug.Log($"Sending ws message {message}");

            try
            {
                await _socket!.SendText(message);
            }
            catch
            {
                Debug.LogError($"Failed to send ws message {message}");
                messageQueue.Enqueue(builder);
            }
        }

        public void Send(OutgoingMessageBuilder messageBuilder) => messageQueue.Enqueue(messageBuilder);

        public void SendImmediate(OutgoingMessageBuilder messageBuilder)
        {
            string message = Jason.Serialize(messageBuilder.GetWsMessage());

            if (_socket?.State is not WebSocketState.Open)
            {
                Debug.LogError($"WS not open - failed to send immediate ws message {message}");
                return;
            }

            Debug.Log($"Sending immediate ws message {message}");

            _socket.SendText(message);
        }

        public static void TrySend(OutgoingMessageBuilder messageBuilder)
        {
            if (Instance == null)
            {
                Debug.LogError("Cannot send message - WebsocketConnection instance is null");
                return;
            }

            Instance.Send(messageBuilder);
        }

        public static void TrySendImmediate(OutgoingMessageBuilder messageBuilder)
        {
            if (Instance == null)
            {
                Debug.LogError("Cannot send immediate message - WebsocketConnection instance is null");
                return;
            }

            Instance.SendImmediate(messageBuilder);
        }

        private async UniTask ReceiveMessage(string msgData)
        {
            try
            {
                await UniTask.SwitchToMainThread();

                Debug.Log("Received ws message " + msgData);

                JObject message = JObject.Parse(msgData);
                string? command = message["command"]?.Value<string>();
                MessageJData data = new(message["data"]);

                if (command == null)
                {
                    Debug.LogError("Received command that could not be deserialized. What the fuck are you doing?");
                    return;
                }

                commandHandler.Handle(command, data);
            }
            catch (Exception e)
            {
                Debug.LogError("Received invalid message");
                Debug.LogError(e);
            }
        }
    }
}
