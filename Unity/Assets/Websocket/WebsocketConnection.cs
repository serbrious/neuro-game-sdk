#nullable enable

using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using NeuroSdk.Messages.API;
using NeuroSdk.Utilities;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WebSocketSharp;

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

        private void StartWs()
        {
            Debug.LogWarning("Initializing WebSocket connection");

            try
            {
                if (_socket?.ReadyState is WebSocketState.Open or WebSocketState.Connecting) _socket.Close();
            }
            catch
            {
                // ignored
            }

            string? websocketUrl = Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.Process) ??
                                   Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.User) ??
                                   Environment.GetEnvironmentVariable("NEURO_SDK_WS_URL", EnvironmentVariableTarget.Machine);
            if (string.IsNullOrEmpty(websocketUrl))
            {
                Debug.LogError("NEURO_SDK_WS_URL environment variable is not set");
                return;
            }

            // Websocket callbacks get run on separate threads! Watch out
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
        }

        private void Update()
        {
            if (_socket?.ReadyState is not WebSocketState.Open) return;

            while (messageQueue.Count > 0)
            {
                OutgoingMessageBuilder builder = messageQueue.Dequeue()!;
                string message = Jason.Serialize(builder.GetWsMessage());

                Debug.Log($"Sending ws message {message}");

                _socket.SendAsync(message, success =>
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

            if (_socket?.ReadyState is not WebSocketState.Open)
            {
                Debug.LogError($"WS not open - failed to send immediate ws message {message}");
                return;
            }

            Debug.Log($"Sending immediate ws message {message}");

            _socket.Send(message);
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
    }
}
