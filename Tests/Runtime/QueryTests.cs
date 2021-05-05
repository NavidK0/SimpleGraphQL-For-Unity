using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace SimpleGraphQL.Tests
{
    public class QueryTests
    {
        // TODO: Ideally mock our own server, but for now just use a publicly available one
        private const string Uri = "https://countries.trevorblades.com/";

        [UnityTest]
        public IEnumerator SimpleQuery()
        {
            var client = new GraphQLClient(Uri);
            var query = new Query { Source = "{ continents { name } }" };
            var responseType = new { continents = new [] { new { name = "" } } };
            var response = client.Send(() => responseType, query);

            yield return response.AsCoroutine();

            Assert.IsNull(response.Result.Errors);

            var data = response.Result.Data;
            Assert.IsNotNull(data);
            Assert.IsNotNull(data.continents);
            Assert.IsTrue(data.continents.Any(c => c.name == "Europe"));
        }

        [UnityTest]
        public IEnumerator FailedQuery()
        {
            var client = new GraphQLClient(Uri);
            var query = new Query { Source = "{ continents MALFORMED name } }" };
            var responseType = new { continents = new [] { new { name = "" } } };
            var response = client.Send(() => responseType, query);

            yield return response.AsCoroutine();

            var errors = response.Result.Errors;
            Assert.IsNotNull(errors);
            Assert.IsNotEmpty(errors);
            Assert.IsNotNull(errors[0].Message);
        }
    }
}
