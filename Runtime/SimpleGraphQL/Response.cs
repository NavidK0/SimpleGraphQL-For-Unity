using System.Runtime.Serialization;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace SimpleGraphQL
{
    [PublicAPI]
    [Preserve]
    public class Response<T>
    {
        [DataMember(Name = "data")]
        public T Data { get; set; }

        [DataMember(Name = "errors")]
        [CanBeNull]
        public Error[] Errors { get; set; }

        [Preserve] // Ensures it survives code-stripping
        public Response()
        {
        }
    }

    [PublicAPI]
    [Preserve]
    public class Error
    {
        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "locations")]
        [CanBeNull]
        public Location[] Locations { get; set; }

        [DataMember(Name = "extensions")]
        [CanBeNull]
        public Extensions Extensions { get; set; }

        [DataMember(Name = "path")]
        [CanBeNull]
        public object[] Path { get; set; } // Path objects can be either integers or strings

        [Preserve] // Ensures it survives code-stripping
        public Error()
        {
        }
    }

    [PublicAPI]
    [Preserve]
    public class Extensions
    {
        [DataMember(Name = "code")]
        public string Code { get; set; }

        [Preserve] // Ensures it survives code-stripping
        public Extensions()
        {
        }
    }

    [PublicAPI]
    [Preserve]
    public class Location
    {
        [DataMember(Name = "line")]
        public int Line { get; set; }

        [DataMember(Name = "column")]
        public int Column { get; set; }

        [Preserve] // Ensures it survives code-stripping
        public Location()
        {
        }
    }
}