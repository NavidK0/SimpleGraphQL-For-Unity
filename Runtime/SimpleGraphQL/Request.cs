using System;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SimpleGraphQL
{
    [Serializable]
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Request
    {
        public string Query { get; set; }

        [CanBeNull]
        public string OperationName { get; set; }

        public object Variables { get; set; }

        public override string ToString()
        {
            return $"GraphQL Request:\n{this.ToJson(true)}";
        }
    }

    [PublicAPI]
    public static class RequestExtensions
    {
        private static JsonSerializerSettings defaultSerializerSettings = new JsonSerializerSettings
            { NullValueHandling = NullValueHandling.Ignore };

        public static byte[] ToBytes(this Request request, JsonSerializerSettings serializerSettings = null)
        {
            return Encoding.UTF8.GetBytes(request.ToJson(false, serializerSettings));
        }

        public static string ToJson(this Request request, bool prettyPrint = false,
            JsonSerializerSettings serializerSettings = null)
        {
            if (serializerSettings == null)
            {
                serializerSettings = defaultSerializerSettings;
            }

            return JsonConvert.SerializeObject
            (request,
                prettyPrint ? Formatting.Indented : Formatting.None,
                serializerSettings
            );
        }
    }
}