using System;
using System.Collections.Generic;
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
        private static ClientWebSocket _webSocket;

        /// <summary>
        /// Called when the websocket receives subscription data.
        /// </summary>
        public static event Action<string> SubscriptionDataReceived;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void PreInit()
        {
            _webSocket?.Dispose();
            SubscriptionDataReceived = null;
        }

        /// <summary>
        /// POST a query to the given endpoint url.
        /// </summary>
        /// <param name="url">The endpoint url.</param>
        /// <param name="query">The query</param>
        /// <param name="authScheme">The authentication scheme to be used.</param>
        /// <param name="authToken">The actual auth token.</param>
        /// <param name="variables">Any variables you want to pass in</param>
        /// <param name="headers">Any headers that should be passed in</param>
        /// <returns></returns>
        public static async Task<string> PostQueryAsync(
            string url,
            Query query,
            string authScheme = "Bearer",
            string authToken = null,
            Dictionary<string, object> variables = null,
            Dictionary<string, string> headers = null
        )
        {
            var uri = new Uri(url);

            byte[] payload = query.ToBytes(variables);

            var request = new UnityWebRequest(uri, "POST")
            {
                uploadHandler = new UploadHandlerRaw(payload),
                downloadHandler = new DownloadHandlerBuffer()
            };

            if (authToken != null)
                request.SetRequestHeader("Authorization", $"{authScheme} {authToken}");

            request.SetRequestHeader("Content-Type", "application/json");

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            try
            {
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

        public static bool IsWebSocketReady() =>
            _webSocket?.State == WebSocketState.Connecting || _webSocket?.State == WebSocketState.Open;

        /// <summary>
        /// Connect to the GraphQL server. Call is necessary in order to send subscription queries via WebSocket.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="authScheme"></param>
        /// <param name="authToken"></param>
        /// <param name="headers"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static async Task WebSocketConnect(
            string url,
            string authScheme = "Bearer",
            string authToken = null,
            string protocol = "graphql-ws",
            Dictionary<string, string> headers = null
        )
        {
            url = url.Replace("http", "ws");

            var uri = new Uri(url);
            _webSocket = new ClientWebSocket();
            _webSocket.Options.AddSubProtocol(protocol);

            if (authToken != null)
                _webSocket.Options.SetRequestHeader("X-Authorization", $"{authScheme} {authToken}");

            _webSocket.Options.SetRequestHeader("Content-Type", "application/json");

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    _webSocket.Options.SetRequestHeader(header.Key, header.Value);
                }
            }

            try
            {
                Debug.Log("Websocket is connecting");
                await _webSocket.ConnectAsync(uri, CancellationToken.None);

                Debug.Log("Websocket is initting");
                // Initialize the socket at the server side
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(Encoding.ASCII.GetBytes(@"{""type"":""connection_init""}")),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );

                Debug.Log("Websocket is updating");
                // Start listening to the websocket for data.
                WebSocketUpdate();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        /// <summary>
        /// Disconnect the websocket.
        /// </summary>
        /// <returns></returns>
        public static async Task WebSocketDisconnect()
        {
            if (_webSocket?.State != WebSocketState.Open)
            {
                Debug.LogError("Attempted to disconnect from a socket that was not open!");
                return;
            }

            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Socket closed.", CancellationToken.None);
        }

        /// <summary>
        /// Subscribe to a query.
        /// </summary>
        /// <param name="id">Used to identify the subscription. Must be unique per query.</param>
        /// <param name="query">The subscription query.</param>
        /// <param name="variables"></param>
        /// <returns>true if successful</returns>
        public static async Task<bool> WebSocketSubscribe(
            string id,
            Query query,
            Dictionary<string, object> variables
        )
        {
            if (!IsWebSocketReady())
            {
                Debug.LogError("Attempted to subscribe to a query without connecting to a WebSocket first!");
                return false;
            }

            string json = JsonConvert.SerializeObject(
                new
                {
                    id,
                    type = "start",
                    payload = new
                    {
                        query = query.Source,
                        variables,
                        operationName = query.OperationName
                    }
                },
                Formatting.None,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }
            );

            await _webSocket.SendAsync(
                new ArraySegment<byte>(Encoding.ASCII.GetBytes(json)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );

            return true;
        }

        /// <summary>
        /// Unsubscribe from this query.
        /// </summary>
        /// <param name="id">Used to identify the subscription. Must be unique per query.</param>
        /// <returns></returns>
        public static async Task WebSocketUnsubscribe(string id)
        {
            if (!IsWebSocketReady())
            {
                Debug.LogError("Attempted to unsubscribe to a query without connecting to a WebSocket first!");
                return;
            }

            await _webSocket.SendAsync(
                new ArraySegment<byte>(Encoding.ASCII.GetBytes($@"{{""type"":""stop"",""id"":""{id}""}}")),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }

        private static async void WebSocketUpdate()
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
                var jsonBuild = new StringBuilder();

                do
                {
                    wsReceiveResult = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);

                    jsonBuild.Append(Encoding.UTF8.GetString(buffer.Array, buffer.Offset, wsReceiveResult.Count));
                } while (!wsReceiveResult.EndOfMessage);

                var jsonResult = jsonBuild.ToString();
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
                    case "connection_error":
                    {
                        throw new WebSocketException("Connection error. Error: " + jsonResult);
                    }
                    case "connection_ack":
                    {
                        Debug.Log("Websocket connection acknowledged.");
                        continue;
                    }
                    case "data":
                    {
                        JToken jToken = jsonObj["payload"];

                        if (jToken != null)
                        {
                            SubscriptionDataReceived?.Invoke(jToken.ToString());
                        }

                        continue;
                    }
                    case "error":
                    {
                        throw new WebSocketException("Handshake error Error: " + jsonResult);
                    }
                    case "complete":
                    {
                        Debug.Log("Server sent complete, it's done sending data.");
                        break;
                    }
                    case "ka":
                    {
                        // stayin' alive, stayin' alive
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
    }
}
