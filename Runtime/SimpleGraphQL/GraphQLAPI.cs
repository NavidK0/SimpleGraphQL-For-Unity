using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Internal;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace SimpleGraphQL
{
    [PublicAPI]
    public class GraphQL
    {
        public string Endpoint;
        public string AuthToken;
        public Query[] SearchableQueries;
        public (string header, string value)[] CustomHeaders;

        public GraphQL(string endpoint, string authToken = null, IEnumerable<Query> queries = null,
            IEnumerable<(string header, string value)> headers = null)
        {
            Endpoint = endpoint;
            AuthToken = authToken;
            SearchableQueries = queries?.ToArray();
            CustomHeaders = headers?.ToArray();
        }

        public GraphQL(GraphQLConfig config)
        {
            Endpoint = config.Endpoint;
            AuthToken = config.AuthToken;
            SearchableQueries = config.Files.SelectMany(x => x.Queries).ToArray();
            CustomHeaders = config.CustomHeaders.Select(x => (x.Key, x.Value)).ToArray();
        }

        public async Task<string> SendAsync(Query query, Dictionary<string, string> variables = null,
            Dictionary<string, string> headers = null)
        {
            if (query.OperationType == OperationType.Subscription)
            {
                Debug.LogError("Operation Type should not be a subscription!");
                return null;
            }

            if (CustomHeaders != null && headers != null)
            {
                foreach ((string header, string value) in CustomHeaders)
                {
                    headers.Add(header, value);
                }
            }

            byte[] bytes = QueryToBytes(query, variables);
            UnityWebRequest request = await HttpUtils.PostAsync(Endpoint, bytes, headers);
            string downloadHandlerText = request.downloadHandler.text;

            return downloadHandlerText;
        }

        public async Task SubscribeAsync(Query query, Dictionary<string, string> variables = null,
            Dictionary<string, string> headers = null)
        {
            if (query.OperationType != OperationType.Subscription)
            {
                Debug.LogError("Operation Type should be a subscription!");
                return;
            }


            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the first query located in a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Query FindQueryByFileName(string fileName)
        {
            return SearchableQueries?.FirstOrDefault(x => x.FileName == fileName);
        }

        /// <summary>
        /// Searches for all queries within a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Query[] FindQueriesByFileName(string fileName)
        {
            return SearchableQueries?.FindAll(x => x.FileName == fileName);
        }

        /// <summary>
        /// Finds the first query with the given operation name.
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public Query FindQueryByOperation(string operation)
        {
            return SearchableQueries?.FirstOrDefault(x => x.OperationName == operation);
        }

        /// <summary>
        /// Finds all queries with the given operation name.
        /// You may need to do additional filtering to get the query you want since they will all have the same operation name.
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public Query[] FindQueriesByOperation(string operation)
        {
            return SearchableQueries?.FindAll(x => x.OperationName == operation);
        }

        public static byte[] QueryToBytes(Query query, Dictionary<string, string> variables = null)
        {
            return Encoding.ASCII.GetBytes(QueryToJson(query, variables));
        }

        public static string QueryToJson(Query query, Dictionary<string, string> variables = null)
        {
            return JsonConvert.SerializeObject
            (
                new {query = query.Source, operationName = query.OperationName, variables},
                Formatting.None,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}
            );
        }
    }
}