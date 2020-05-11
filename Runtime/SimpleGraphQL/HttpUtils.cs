using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SimpleGraphQL
{
    [PublicAPI]
    public static class HttpUtils
    {
        public static event Action<string> SubscriptionDataReceived;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void PreInit()
        {
            SubscriptionDataReceived = null;
        }

        public static async Task<string> PostQueryAsync(
            string url,
            byte[] payload,
            string authScheme = "Bearer",
            string authToken = null,
            Dictionary<string, string> headers = null
        )
        {
            var uri = new Uri(url);

            var request = new UnityWebRequest(uri, "POST")
            {
                uploadHandler = new UploadHandlerRaw(payload),
                downloadHandler = new DownloadHandlerBuffer()
            };

            try
            {
                request.SetRequestHeader("Content-Type", "application/json");

                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        request.SetRequestHeader(header.Key, header.Value);
                    }
                }

                request.SendWebRequest();

                while (!request.isDone)
                {
                    await Task.Yield();
                }

                return request.downloadHandler.text;
            }
            catch (Exception e)
            {
                Debug.LogError("[SimpleGraphQL] " + e);
                return null;
            }
        }

        public static async Task<WebSocket> WebSocketConnect(
            string url,
            string query,
            bool secure = true,
            string socketId = "1",
            string protocol = "graphql-ws"
        )
        {
            url = url
                .Replace("http", secure ? "wss" : "ws")
                .Replace("https", secure ? "wss" : "ws");

            var uri = new Uri(url);
            var socket = new ClientWebSocket();

            socket.Options.AddSubProtocol(protocol);

            try
            {
                await socket.ConnectAsync(uri, CancellationToken.None);

                if (socket.State == WebSocketState.Open)
                    Debug.Log("[SimpleGraphQL] WebSocket open.");
                else
                {
                    Debug.Log("[SimpleGraphQ] WebSocket is not open: STATE: " + socket.State);
                }

                await WebSocketInit(socket);

                // Start listening to the websocket for data.
                WebSocketUpdate(socket);

                await WebSocketSend(socket, socketId, query);
            }
            catch (Exception e)
            {
                Debug.LogError("[SimpleGraphQL] " + e.Message);
            }

            return socket;
        }

        private static async Task WebSocketInit(WebSocket socket)
        {
            await socket.SendAsync(
                new ArraySegment<byte>(
                    Encoding.ASCII.GetBytes(@"{""type"":""connection_init""}")
                ),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        private static async Task WebSocketSend(WebSocket socket, string id, string query)
        {
            string json = JsonConvert.SerializeObject(new {id, type = "start", payload = new {query}});

            await socket.SendAsync(
                new ArraySegment<byte>(Encoding.ASCII.GetBytes(json)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }

        private static async void WebSocketUpdate(WebSocket socket)
        {
            while (true)
            {
                ArraySegment<byte> buffer;
                buffer = WebSocket.CreateClientBuffer(1024, 1024);

                if (buffer.Array == null)
                {
                    throw new WebSocketException("Buffer array is null!");
                }

                WebSocketReceiveResult wsReceiveResult;
                var jsonResult = "";

                do
                {
                    wsReceiveResult = await socket.ReceiveAsync(buffer, CancellationToken.None);

                    jsonResult += Encoding.UTF8.GetString(buffer.Array, buffer.Offset, wsReceiveResult.Count);
                } while (!wsReceiveResult.EndOfMessage);

                if (jsonResult.IsNullOrEmpty()) return;

                JObject jsonObj;

                try
                {
                    jsonObj = JObject.Parse(jsonResult);
                }
                catch (JsonReaderException e)
                {
                    throw new ApplicationException(e.Message);
                }

                var subType = (string) jsonObj["type"];
                switch (subType)
                {
                    case "connection_ack":
                    {
                        Debug.Log("[SimpleGraphQL] Successfully connected to WebSocket.");
                        continue;
                    }
                    case "error":
                    {
                        throw new WebSocketException("Handshake error Error: " + jsonResult);
                    }
                    case "connection_error":
                    {
                        throw new WebSocketException("Connection error. Error: " + jsonResult);
                    }
                    case "data":
                    {
                        SubscriptionDataReceived?.Invoke(jsonResult);
                        continue;
                    }
                    case "ka":
                    {
                        continue;
                    }
                    case "subscription_fail":
                    {
                        throw new WebSocketException("Subscription failed. Error: " + jsonResult);
                    }
                }

                break;
            }
        }

        public static async Task WebsocketDisconnect(ClientWebSocket webSocket, string socketId = "1")
        {
            var buffer =
                new ArraySegment<byte>(
                    Encoding.ASCII.GetBytes($@"{{""type"":""stop"",""id"":""{socketId}""}}")
                );

            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Socket closed.", CancellationToken.None);
        }
    }
}