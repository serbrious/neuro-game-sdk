#nullable enable

using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using NeuroSdk.Messages.API;
using NeuroSdk.Utilities;
using Newtonsoft.Json.Linq;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using NativeWebSocket;
using System.Linq;
using UnityEngine.Networking;
#else
using WebSocketSharp;
#endif

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

        private void Start() => StartWs();

        private async UniTask Reconnect()
        {
            await UniTask.SwitchToMainThread();
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            StartWs();
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private async void StartWs()
#else
        private void StartWs()
#endif
        {
            Debug.LogWarning("Initializing WebSocket connection");

            try
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                if (_socket?.State is WebSocketState.Open or WebSocketState.Connecting) await _socket.Close();
#else
                if (_socket?.ReadyState is WebSocketState.Open or WebSocketState.Connecting) _socket.Close();
#endif
            }
            catch
            {
                // ignored
            }

            string? websocketUrl = null;

#if UNITY_WEBGL && !UNITY_EDITOR
            if (Application.absoluteURL.IndexOf("?") != -1)
            {
                string[] tempStr = Application.absoluteURL.Split('?')[1].Split("WebSocketURL=");
                if (tempStr.Length > 1)
                {
                    string? urlParameter = tempStr[1].Split('&')[0];
                    if (!string.IsNullOrEmpty(urlParameter))
                    {
                        websocketUrl = urlParameter;
                    }
                }
            }

            if (string.IsNullOrEmpty(websocketUrl))
            {
                string[] urlParts = Application.absoluteURL.Split(':');
                string baseUrl = urlParts[0];
                if (urlParts.Length > 1)
                    baseUrl += ":" + new string(urlParts[1].SkipWhile(c => !char.IsDigit(c)).TakeWhile(c => char.IsDigit(c)).ToArray());

                UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/$env/NEURO_SDK_WS_URL");
                try
                {
                    await request.SendWebRequest();
                    if (request.result == UnityWebRequest.Result.Success)
                        websocketUrl = request.downloadHandler.text;
                }
                catch
                {
                    // ignored
                }
            }

            if (!string.IsNullOrEmpty(websocketUrl))
            {
                Environment.SetEnvironmentVariable("NEURO_SDK_WS_URL", websocketUrl, EnvironmentVariableTarget.Process);
            }
#endif

            websocketUrl = Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.Process) ??
                       Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.User) ??
                       Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.Machine);
            if (string.IsNullOrEmpty(websocketUrl))
            {
                Debug.LogError("NEURO_SDK_WS_URL environment variable is not set");
                return;
            }

            // Websocket callbacks get run on separate threads! Watch out
#if UNITY_WEBGL && !UNITY_EDITOR
            _socket = new WebSocket(websocketUrl);
            _socket.OnMessage += (bytes) =>
            {
                Debug.LogWarning(bytes);
                string msg = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.LogWarning(msg);
                ReceiveMessage(msg).Forget();
            };
            _socket.OnError += (e) =>
            {
                Debug.LogError("Websocket connection has encountered an error!");
                Debug.LogError(e);
                Reconnect().Forget();
            };
            _socket.OnClose += (_) =>
            {
                Debug.LogError("Websocket connection has been closed!");
                Reconnect().Forget();
            };
            _ = _socket.Connect();
#else
            _socket = new WebSocket(websocketUrl);
            _socket.OnMessage += (_, msg) =>
            {
                ReceiveMessage(msg).Forget();
            };
            _socket.OnError += (_, e) =>
            {
                Debug.LogError("Websocket connection has encountered an error!");
                Debug.LogError(e.Message);
                Debug.LogError(e.Exception);
                Reconnect().Forget();
            };
            _socket.OnClose += (_, _) =>
            {
                Debug.LogError("Websocket connection has been closed!");
                Reconnect().Forget();
            };
            _socket.ConnectAsync();
#endif
        }

        private void Update()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (_socket?.State is not WebSocketState.Open) return;
#else
            if (_socket?.ReadyState is not WebSocketState.Open) return;
#endif

            while (messageQueue.Count > 0)
            {
                OutgoingMessageBuilder builder = messageQueue.Dequeue()!;
                string message = Jason.Serialize(builder.GetWsMessage());

                Debug.Log($"Sending ws message {message}");

                SendAsyncMessage(message, success =>
                {
                    if (!success)
                    {
                        Debug.LogError($"Failed to send ws message {message}");
                        messageQueue.Enqueue(builder);
                    }
                });
            }
        }

        public void Send(OutgoingMessageBuilder messageBuilder) => messageQueue.Enqueue(messageBuilder);

        public static void SendImmediate(OutgoingMessageBuilder messageBuilder)
        {
            string message = Jason.Serialize(messageBuilder.GetWsMessage());

#if UNITY_WEBGL && !UNITY_EDITOR
            if (_socket?.State is not WebSocketState.Open)
#else
            if (_socket?.ReadyState is not WebSocketState.Open)
#endif
            {
                Debug.LogError($"WS not open - failed to send immediate ws message {message}");
                return;
            }

            Debug.Log($"Sending immediate ws message {message}");

            SendImmediateMessage(message);
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

#if UNITY_WEBGL && !UNITY_EDITOR
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
#else
        private async UniTask ReceiveMessage(MessageEventArgs msg)
        {
            try
            {
                await UniTask.SwitchToMainThread();

                Debug.Log("Received ws message " + msg.Data);

                JObject message = JObject.Parse(msg.Data);
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
#endif

        private static void SendImmediateMessage(string message)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SendAsyncMessage(message, success => { });
#else
            _socket?.Send(message);
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private static async void SendAsyncMessage(string message, Action<bool> callback)
#else
        private static void SendAsyncMessage(string message, Action<bool> callback)
#endif
        {
            if (_socket == null) return;

#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                await _socket.SendText(message);
                callback(true);
            }
            catch
            {
                callback(false);
            }
#else
            _socket.SendAsync(message, callback);
#endif
        }
    }
}
