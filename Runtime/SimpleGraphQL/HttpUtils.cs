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
    public enum SubscriptionError
    {
        SocketFailure,
        InvalidPayload
    }

    [PublicAPI]
    public static class HttpUtils
    {
        public interface IWebSocket : IDisposable
        {
            public interface ISocketOptions
            {
                public void AddSubProtocol(string protocol);

                public void SetRequestHeader(string v1, string v2);
            }

            WebSocketState State { get; }
            ISocketOptions Options { get; }
            WebSocketCloseStatus? CloseStatus { get; }

            Task ConnectAsync(Uri uri, CancellationToken none);
            Task SendAsync(ArraySegment<byte> arraySegment, WebSocketMessageType text, bool v, CancellationToken ct);
            Task CloseAsync(WebSocketCloseStatus normalClosure, string v, CancellationToken ct);
            Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken ct);
        }


        public class ClientWebSocketOptionsWrapper : IWebSocket.ISocketOptions
        {
            ClientWebSocketOptions o;

            public ClientWebSocketOptionsWrapper(ClientWebSocketOptions o_)
            {
                o = o_;
            }

            public void AddSubProtocol(string protocol)
            {
                o.AddSubProtocol(protocol);
            }

            public void SetRequestHeader(string v1, string v2)
            {
                o.SetRequestHeader(v1, v2);
            }
        }


        public class ClientWebSocketWrapper : IWebSocket
        {
            ClientWebSocket ws;

            public ClientWebSocketWrapper()
            {
                ws = new ClientWebSocket();
            }

            WebSocketState IWebSocket.State => ws.State;

            public WebSocketCloseStatus? CloseStatus => ws.CloseStatus;

            public IWebSocket.ISocketOptions Options => new ClientWebSocketOptionsWrapper(ws.Options);


            public Task ConnectAsync(Uri uri, CancellationToken ct)
            {
                return ws.ConnectAsync(uri, ct);
            }

            public Task SendAsync(ArraySegment<byte> arraySegment, WebSocketMessageType text, bool v, CancellationToken ct)
            {
                return ws.SendAsync(arraySegment, text, v, ct);
            }

            public Task CloseAsync(WebSocketCloseStatus normalClosure, string v, CancellationToken ct)
            {
                return ws.CloseAsync(normalClosure, v, ct);
            }

            public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken ct)
            {
                return ws.ReceiveAsync(buffer, ct);
            }

            public void Dispose()
            {
                ws.Dispose();
            }
        }

        private static IWebSocket _webSocket;

        /// <summary>
        /// Called when the websocket receives subscription data.
        /// </summary>
        internal static event Action<string> SubscriptionDataReceived;

        /// <summary>
        /// Called when the an error occurs during websocket operations.
        /// </summary>
        internal static event Action<SubscriptionError, string> SubscriptionErrorOccured;

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
            Uri uri = new Uri(url);

            byte[] payload = query.ToBytes(variables);

            UnityWebRequest request = new UnityWebRequest(uri, "POST")
            {
                uploadHandler = new UploadHandlerRaw(payload),
                downloadHandler = new DownloadHandlerBuffer()
            };

            if(authToken != null)
            {
                request.SetRequestHeader("Authorization", $"{authScheme} {authToken}");
            }

            request.SetRequestHeader("Content-Type", "application/json");

            if(headers != null)
            {
                foreach(KeyValuePair<string, string> header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            try
            {
                request.SendWebRequest();

                while(!request.isDone)
                {
                    await Task.Yield();
                }

                return request.downloadHandler.text;
            }
            catch(Exception e)
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
        public static async Task<bool> WebSocketConnect(
            string url,
            string authScheme = "Bearer",
            string authToken = null,
            string protocol = "graphql-ws",
            Dictionary<string, string> headers = null,
            IWebSocket ws = null
        )
        {
            url = url.Replace("http", "ws");

            Uri uri = new Uri(url);
            if (ws != null) {
                _webSocket = ws;
            }
            else
            {
                _webSocket = new ClientWebSocketWrapper();
            }
            _webSocket.Options.AddSubProtocol(protocol);

            if(authToken != null)
            {
                _webSocket.Options.SetRequestHeader("X-Authorization", $"{authScheme} {authToken}");
            }

            _webSocket.Options.SetRequestHeader("Content-Type", "application/json");

            if(headers != null)
            {
                foreach(KeyValuePair<string, string> header in headers)
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
                return true;
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Disconnect the websocket.
        /// </summary>
        /// <returns></returns>
        public static async Task WebSocketDisconnect()
        {
            if(_webSocket?.State != WebSocketState.Open)
            {
                Debug.LogError("Attempted to disconnect from a socket that was not open!");
                return;
            }

            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Socket closed.", CancellationToken.None);
            ClearWebSocket();
        }

        private static void ClearWebSocket()
        {
            _webSocket?.Dispose();
            _webSocket = null;
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
            if(!IsWebSocketReady())
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

            try
            {
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(Encoding.ASCII.GetBytes(json)),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );

                return true;
            }
            catch(Exception e)
            {
                Debug.LogError($"Subscribe failed:\nSocket state: {_webSocket?.State.ToString() ?? "N/A"}\nClose status: {_webSocket?.CloseStatus?.ToString() ?? "N/A"}\nError message: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unsubscribe from this query.
        /// </summary>
        /// <param name="id">Used to identify the subscription. Must be unique per query.</param>
        /// <returns></returns>
        public static async Task WebSocketUnsubscribe(string id)
        {
            if(!IsWebSocketReady())
            {
                Debug.LogError("Attempted to unsubscribe to a query without connecting to a WebSocket first!");
                return;
            }

            try
            {
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(Encoding.ASCII.GetBytes($@"{{""type"":""stop"",""id"":""{id}""}}")),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
            catch(Exception e)
            {
                Debug.LogError($"Unsubscribe failed:\nSocket state: {_webSocket?.State.ToString() ?? "N/A"}\nClose status: {_webSocket?.CloseStatus?.ToString() ?? "N/A"}\nError message: {e.Message}");
            }
        }

        private static async void WebSocketUpdate()
        {
            while(true)
            {
                ArraySegment<byte> buffer;
                buffer = WebSocket.CreateClientBuffer(1024, 1024);

                if(buffer.Array == null)
                {
                    throw new WebSocketException("Buffer array is null!");
                }

                WebSocketReceiveResult wsReceiveResult;
                StringBuilder jsonBuild = new StringBuilder();

                try
                {
                    do
                    {
                        wsReceiveResult = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);

                        jsonBuild.Append(Encoding.UTF8.GetString(buffer.Array, buffer.Offset, wsReceiveResult.Count));
                    } while(!wsReceiveResult.EndOfMessage);
                }
                catch(Exception e)
                {
                    Debug.LogError($"Socket failure:\n{e.Message}");
                    ClearWebSocket();
                    SubscriptionErrorOccured?.Invoke(SubscriptionError.SocketFailure, e.ToString());
                    break;
                }

                string jsonResult = jsonBuild.ToString();
                if(string.IsNullOrEmpty(jsonResult))
                {
                    return;
                }

                JObject jsonObj;
                try
                {
                    jsonObj = JObject.Parse(jsonResult);
                }
                catch(JsonReaderException e)
                {
                    Debug.LogError($"Socket failure:\n{e.Message}");
                    await WebSocketDisconnect();
                    SubscriptionErrorOccured?.Invoke(SubscriptionError.InvalidPayload, e.ToString());
                    break;
                }

                string subType = (string)jsonObj["type"];
                switch(subType)
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

                        if(jToken != null)
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
