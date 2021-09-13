using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using Newtonsoft.Json;

namespace SimpleGraphQL.Tests
{
    public class QueryTests
    {
        // TODO: Ideally mock our own server, but for now just use a publicly available one
        private const string Uri = "https://countries.trevorblades.com";

        [UnityTest]
        public IEnumerator SimpleQuery()
        {
            var client = new GraphQLClient(Uri);
            var request = new Request { Query = "{ continents { name } }" };
            var responseType = new { continents = new[] { new { name = "" } } };
            var response = client.Send(() => responseType, request);

            yield return response.AsCoroutine();

            Assert.IsNull(response.Result.Errors);

            var data = response.Result.Data;
            Assert.IsNotNull(data);
            Assert.IsNotNull(data.continents);
            Assert.IsTrue(data.continents.Any(c => c.name == "Europe"));
        }

        [UnityTest]
        public IEnumerator MalformedQuery()
        {
            var client = new GraphQLClient(Uri);
            var query = new Request { Query = "{ continents MALFORMED name } }" };
            var responseType = new { continents = new[] { new { name = "" } } };
            var response = client.Send(() => responseType, query);

            yield return response.AsCoroutineUnchecked();
            var ex = response.Expect<UnityWebRequestException>();

            Assert.AreEqual(ex.ResponseCode, 400); // Bad request

            Error[] errors = DeserializeResponse(() => responseType, ex.Text).Errors;

            Assert.IsNotNull(errors);
            Assert.IsNotEmpty(errors);
            Assert.IsNotNull(errors[0].Message);
        }

        // Could be part of the library?
        private Response<T> DeserializeResponse<T>(Func<T> responseTypeResolver, string json)
        {
            return JsonConvert.DeserializeObject<Response<T>>(json);
        }

        [UnityTest]
        public IEnumerator QueryWithArgs()
        {
            var client = new GraphQLClient(Uri);
            var request = new Request
            {
                Query = "query ContinentNameByCode($code: ID!) { continent(code: $code) { name } }",
                Variables = new
                {
                    code = "EU"
                }
            };
            var responseType = new { continent = new { name = "" } };
            var response = client.Send(() => responseType, request);

            yield return response.AsCoroutine();

            Assert.IsNull(response.Result.Errors);

            var data = response.Result.Data;
            Assert.IsNotNull(data);
            Assert.IsNotNull(data.continent);
            Assert.AreEqual(data.continent.name, "Europe");
        }

        [UnityTest]
        public IEnumerator NetworkError()
        {
            var client = new GraphQLClient("https://non_resolvable_host_123123123123123");
            var request = new Request { Query = "{ continents { name } }" };
            var responseType = new { continents = new[] { new { name = "" } } };
            var response = client.Send(() => responseType, request);

            yield return response.AsCoroutineUnchecked();
            var ex = response.Expect<UnityWebRequestException>();
#if UNITY_2020_2_OR_NEWER
            Assert.AreEqual(ex.Result, UnityWebRequest.Result.ConnectionError);
#else
            Assert.IsTrue(ex.IsNetworkError);
#endif
        }

        [UnityTest]
        public IEnumerator Http404Error()
        {
            var client = new GraphQLClient("https://google.com/url_that_returns_404_lsdfksadjflksdafjs");
            var request = new Request { Query = "{ continents { name } }" };
            var responseType = new { continents = new[] { new { name = "" } } };
            var response = client.Send(() => responseType, request);

            yield return response.AsCoroutineUnchecked();
            var ex = response.Expect<UnityWebRequestException>();
#if UNITY_2020_2_OR_NEWER
            Assert.AreEqual(ex.Result, UnityWebRequest.Result.ProtocolError);
#else
            Assert.IsTrue(ex.IsHttpError);
#endif
            Assert.AreEqual(ex.ResponseCode, 404);
        }
    }
}