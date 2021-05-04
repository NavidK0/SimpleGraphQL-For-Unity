using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace SimpleGraphQL
{
    /// <summary>
    /// This API object is meant to be reused, so keep an instance of it somewhere!
    /// </summary>
    [PublicAPI]
    public class GraphQLClient
    {
        public readonly List<Query> SearchableQueries;
        public readonly Dictionary<string, string> CustomHeaders;
        public string Endpoint;
        public string AuthScheme;

        public GraphQLClient(
            string endpoint,
            IEnumerable<Query> queries = null,
            Dictionary<string, string> headers = null,
            string authScheme = null
        )
        {
            Endpoint = endpoint;
            AuthScheme = authScheme;
            SearchableQueries = queries?.ToList();
            CustomHeaders = headers;
        }

        public GraphQLClient(GraphQLConfig config)
        {
            Endpoint = config.Endpoint;
            SearchableQueries = config.Files.SelectMany(x => x.Queries).ToList();
            CustomHeaders = config.CustomHeaders.ToDictionary(header => header.Key, header => header.Value);
            AuthScheme = config.AuthScheme;
        }


        /// <summary>
        /// Send a query!
        /// </summary>
        /// <param name="query">The query you are sending. These should be generated from your graphQL files.</param>
        /// <param name="variables">Any variables you want to pass</param>
        /// <param name="headers">Any headers you want to pass</param>
        /// <param name="authToken">The authToken</param>
        /// <param name="authScheme">The authScheme to be used.</param>
        /// <returns></returns>
        public async Task<string> Send(
            Query query,
            Dictionary<string, object> variables = null,
            Dictionary<string, string> headers = null,
            string authToken = null,
            string authScheme = null
        )
        {
            if (query.OperationType == OperationType.Subscription)
            {
                Debug.LogError("Operation Type should not be a subscription!");
                return null;
            }

            if (CustomHeaders != null)
            {
                if (headers == null) headers = new Dictionary<string, string>();

                foreach (KeyValuePair<string, string> header in CustomHeaders)
                {
                    headers.Add(header.Key, header.Value);
                }
            }

            if (authScheme == null)
            {
                authScheme = AuthScheme;
            }

            string postQueryAsync = await HttpUtils.PostQueryAsync(
                Endpoint,
                query,
                variables,
                headers,
                authToken,
                authScheme
            );

            return postQueryAsync;
        }

        public async Task<Response<TResponse>> Send<TResponse>(
            Query query,
            Dictionary<string, object> variables = null,
            Dictionary<string, string> headers = null,
            string authToken = null,
            string authScheme = null
            )
        {
            var json = await Send(query, variables, headers, authToken, authScheme);
            return JsonConvert.DeserializeObject<Response<TResponse>>(json);
        }

        /// <summary>
        /// Send a query!
        /// </summary>
        /// <param name="query">The query you are sending. These should be generated from your graphQL files.</param>
        /// <param name="variables">Any variables you want to pass</param>
        /// <param name="headers">Any headers you want to pass</param>
        /// <param name="authToken">The authToken</param>
        /// <param name="authScheme">The authScheme to be used.</param>
        /// <returns></returns>
        [Obsolete("SendAsync is deprecated, please use Send instead.")]
        public async Task<string> SendAsync(
            Query query,
            Dictionary<string, object> variables = null,
            Dictionary<string, string> headers = null,
            string authToken = null,
            string authScheme = null
        )
        {
            return await Send(query, variables, headers, authScheme, authScheme);
        }

        /// <summary>
        /// Registers a listener for subscriptions.
        /// </summary>
        /// <param name="listener"></param>
        public void RegisterListener(Action<string> listener)
        {
            HttpUtils.SubscriptionDataReceived += listener;
        }

        /// <summary>
        /// Unregisters a listener for subscriptions.
        /// </summary>
        /// <param name="listener"></param>
        public void UnregisterListener(Action<string> listener)
        {
            HttpUtils.SubscriptionDataReceived -= listener;
        }

        /// <summary>
        /// Subscribe to a query in GraphQL.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="variables"></param>
        /// <param name="headers"></param>
        /// <param name="authToken"></param>
        /// <param name="authScheme"></param>
        /// <returns>True if successful</returns>
        public async Task<bool> Subscribe(
            Query query,
            Dictionary<string, object> variables = null,
            Dictionary<string, string> headers = null,
            string authToken = null,
            string authScheme = null
        )
        {
            if (query.OperationType != OperationType.Subscription)
            {
                Debug.LogError("Operation Type should be a subscription!");
                return false;
            }

            if (CustomHeaders != null)
            {
                if (headers == null) headers = new Dictionary<string, string>();

                foreach (KeyValuePair<string, string> header in CustomHeaders)
                {
                    headers.Add(header.Key, header.Value);
                }
            }

            if (authScheme == null)
            {
                authScheme = AuthScheme;
            }

            if (!HttpUtils.IsWebSocketReady())
            {
                // Prepare the socket before continuing.
                await HttpUtils.WebSocketConnect(Endpoint, "graphql-ws", headers, authToken, authScheme);
            }

            return await HttpUtils.WebSocketSubscribe(query.ToString(), query, variables);
        }

        /// <summary>
        /// Subscribe to a query in GraphQL.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="id">A custom id to pass.</param>
        /// <param name="variables"></param>
        /// <param name="headers"></param>
        /// <param name="authToken"></param>
        /// <param name="authScheme"></param>
        /// <returns>True if successful</returns>
        public async Task<bool> Subscribe(
            Query query,
            string id,
            Dictionary<string, object> variables = null,
            Dictionary<string, string> headers = null,
            string authToken = null,
            string authScheme = null
        )
        {
            if (query.OperationType != OperationType.Subscription)
            {
                Debug.LogError("Operation Type should be a subscription!");
                return false;
            }

            if (CustomHeaders != null)
            {
                if (headers == null) headers = new Dictionary<string, string>();

                foreach (KeyValuePair<string, string> header in CustomHeaders)
                {
                    headers.Add(header.Key, header.Value);
                }
            }

            if (authScheme == null)
            {
                authScheme = AuthScheme;
            }

            if (!HttpUtils.IsWebSocketReady())
            {
                // Prepare the socket before continuing.
                await HttpUtils.WebSocketConnect(Endpoint, "graphql-ws", headers, authToken, authScheme);
            }

            return await HttpUtils.WebSocketSubscribe(id, query, variables);
        }


        /// <summary>
        /// Subscribe to a query in GraphQL.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="variables"></param>
        /// <param name="headers"></param>
        /// <param name="authToken"></param>
        /// <param name="authScheme"></param>
        /// <returns>True if successful</returns>
        [Obsolete("SubscribeAsync is deprecated, please use Subscribe instead.")]
        public async Task<bool> SubscribeAsync(
            Query query,
            Dictionary<string, object> variables = null,
            Dictionary<string, string> headers = null,
            string authToken = null,
            string authScheme = null
        )
        {
            return await Subscribe(query, variables, headers, authScheme, authScheme);
        }

        /// <summary>
        /// Unsubscribe from a query.
        /// </summary>
        /// <param name="query"></param>
        public async Task Unsubscribe(Query query)
        {
            if (!HttpUtils.IsWebSocketReady())
            {
                // Socket is already apparently closed, so this wouldn't work anyways.
                return;
            }

            await HttpUtils.WebSocketUnsubscribe(query.ToString());
        }

        /// <summary>
        /// Unsubscribe from a query.
        /// </summary>
        /// <param name="id"></param>
        public async Task Unsubscribe(string id)
        {
            if (!HttpUtils.IsWebSocketReady())
            {
                // Socket is already apparently closed, so this wouldn't work anyways.
                return;
            }

            await HttpUtils.WebSocketUnsubscribe(id);
        }

        /// <summary>
        /// Finds the first query located in a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Query FindQuery(string fileName)
        {
            return SearchableQueries?.FirstOrDefault(x => x.FileName == fileName);
        }

        /// <summary>
        /// Finds the first query located in a file.
        /// </summary>
        /// <param name="operationName"></param>
        /// <returns></returns>
        public Query FindQueryByOperation(string operationName)
        {
            return SearchableQueries?.FirstOrDefault(x => x.OperationName == operationName);
        }

        /// <summary>
        /// Finds a query by fileName and operationName.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="operationName"></param>
        /// <returns></returns>
        public Query FindQuery(string fileName, string operationName)
        {
            return SearchableQueries?.FirstOrDefault(x => x.FileName == fileName && x.OperationName == operationName);
        }

        /// <summary>
        /// Finds a query by operationName and operationType.
        /// </summary>
        /// <param name="operationName"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public Query FindQuery(string operationName, OperationType operationType)
        {
            return SearchableQueries?.FirstOrDefault(x =>
                x.OperationName == operationName &&
                x.OperationType == operationType
            );
        }

        /// <summary>
        /// Finds a query by fileName, operationName, and operationType.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="operationName"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public Query FindQuery(string fileName, string operationName, OperationType operationType)
        {
            return SearchableQueries?.FirstOrDefault(
                x => x.FileName == fileName &&
                     x.OperationName == operationName &&
                     x.OperationType == operationType
            );
        }

        /// <summary>
        /// Finds all queries with the given operation name.
        /// You may need to do additional filtering to get the query you want since they will all have the same operation name.
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public List<Query> FindQueriesByOperation(string operation)
        {
            return SearchableQueries?.FindAll(x => x.OperationName == operation);
        }
    }
}