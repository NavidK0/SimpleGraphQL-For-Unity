using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class GraphQL
    {
        public readonly List<Query> SearchableQueries;
        public readonly Dictionary<string, string> CustomHeaders;
        
        public string Endpoint;
        public string AuthScheme;

        public GraphQL(string endpoint, string authScheme = "Bearer", IEnumerable<Query> queries = null,
            Dictionary<string, string> headers = null)
        {
            Endpoint = endpoint;
            AuthScheme = authScheme;
            SearchableQueries = queries?.ToList();
            CustomHeaders = headers;
        }

        public GraphQL(GraphQLConfig config)
        {
            Endpoint = config.Endpoint;
            AuthScheme = config.AuthScheme;
            SearchableQueries = config.Files.SelectMany(x => x.Queries).ToList();
            CustomHeaders = config.CustomHeaders.ToDictionary(header => header.Key, header => header.Value);
        }

        public async Task<string> SendAsync(Query query, string authToken = null,
            Dictionary<string, string> variables = null,
            Dictionary<string, string> headers = null)
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

            byte[] bytes = QueryToBytes(query, variables);
            string postQueryAsync = await HttpUtils.PostQueryAsync(Endpoint, bytes, AuthScheme, authToken, headers);

            return postQueryAsync;
        }

        public async Task SubscribeAsync(Query query, string authToken = null,
            Dictionary<string, string> variables = null,
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
        public List<Query> FindQueriesByFileName(string fileName)
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
        public List<Query> FindQueriesByOperation(string operation)
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