using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace SimpleGraphQL
{
    [PublicAPI]
    public class Response<T>
    {
        [DataMember(Name = "data")]
        public T Data { get; set; }
    }
}