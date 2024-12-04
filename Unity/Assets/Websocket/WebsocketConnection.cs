#nullable enable

using System;
using System.Collections;
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
        public string websocketUrl = null!;

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

        private IEnumerator Reconnect()
        {
            yield return new WaitForSeconds(3);
            StartWs();
        }

        public void StartWs()
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

            _socket = new WebSocket(websocketUrl);
            _socket.OnMessage += (_, msg) => ReceiveMessage(msg);
            _socket.OnError += (_, e) =>
            {
                Debug.LogError("Websocket connection has encountered an error!");
                Debug.LogError(e.Message);
                Debug.LogError(e.Exception);
                StartCoroutine(Reconnect());
            };
            _socket.OnClose += (_, _) =>
            {
                Debug.LogError("Websocket connection has been closed!");
                StartCoroutine(Reconnect());
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

        private void ReceiveMessage(MessageEventArgs msg)
        {
            try
            {
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
