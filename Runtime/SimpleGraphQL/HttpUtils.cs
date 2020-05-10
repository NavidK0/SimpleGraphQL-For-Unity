using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleGraphQL.AsyncAwaitUtil;
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

        public static async Task<UnityWebRequest> PostAsync(
            string url,
            byte[] payload,
            Dictionary<string, string> headers = null
        )
        {
            UnityWebRequest request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(payload);

            if (headers != null)
                foreach (KeyValuePair<string, string> keyValuePair in headers)
                {
                    request.SetRequestHeader(keyValuePair.Key, keyValuePair.Value);
                }

            request.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await request.SendWebRequest();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SimpleGraphQL] " + e);
            }

            if (request.error != null)
            {
                Debug.LogError(request.error);
            }

            return request;
        }

        public static async Task<UnityWebRequest> GetAsync(string url, string authToken = null)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);

            if (!authToken.IsNullOrWhitespace())
                request.SetRequestHeader("Authorization", "Bearer " + authToken);

            try
            {
                await request.SendWebRequest();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SimpleGraphQL] " + e);
            }

            if (request.error != null)
            {
                Debug.LogError(request.error);
            }

            return request;
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
                Debug.LogWarning("[SimpleGraphQL] " + e.Message);
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