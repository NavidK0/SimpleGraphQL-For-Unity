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
        public static byte[] ToBytes(this Request request)
        {
            return Encoding.UTF8.GetBytes(request.ToJson());
        }

        public static string ToJson(this Request request,
            bool prettyPrint = false)
        {
            return JsonConvert.SerializeObject
            (   request,
                prettyPrint ? Formatting.Indented : Formatting.None,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}
            );
        }
    }
}