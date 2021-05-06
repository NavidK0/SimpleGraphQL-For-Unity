using System.Collections;
using System.Threading.Tasks;

namespace SimpleGraphQL.Tests
{
    public static class UnityTestAsyncExtensions
    {
        public static IEnumerator AsCoroutine(this Task task)
        {
            while (!task.IsCompleted) yield return null;
            // Throws if the Task fails
            task.GetAwaiter().GetResult();
        }
    }
}