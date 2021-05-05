using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SimpleGraphQL
{
    [PublicAPI]
    [Serializable]
    public class Query
    {
        /// <summary>
        /// The filename that this query is located in.
        /// This is mostly used for searching and identification purposes, and is not
        /// necessarily needed for dynamically created queries.
        /// </summary>
        [CanBeNull]
        public string FileName;

        /// <summary>
        /// The operation name of this query.
        /// It may be null, in which case it should be the only anonymous query in this file.
        /// </summary>
        [CanBeNull]
        public string OperationName;

        /// <summary>
        /// The type of query this is.
        /// </summary>
        public OperationType OperationType;

        /// <summary>
        /// The actual query itself.
        /// </summary>
        public string Source;

        public override string ToString()
        {
            return $"{FileName}:{OperationName}:{OperationType}";
        }
    }

    [PublicAPI]
    public static class QueryExtensions
    {
        public static byte[] ToBytes(this Query query, object variables = null)
        {
            return Encoding.UTF8.GetBytes(ToJson(query, variables));
        }

        public static string ToJson(this Query query, object variables = null,
            bool prettyPrint = false)
        {
            return JsonConvert.SerializeObject
            (new
                {
                    query = query.Source, operationName = query.OperationName, variables
                },
                prettyPrint ? Formatting.Indented : Formatting.None,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}
            );
        }
    }

    [PublicAPI]
    [Serializable]
    public enum OperationType
    {
        Query,
        Mutation,
        Subscription
    }
}