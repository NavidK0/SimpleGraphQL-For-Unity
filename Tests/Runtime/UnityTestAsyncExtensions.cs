using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

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

        // Doesn't check for errors
        public static IEnumerator AsCoroutineUnchecked(this Task task)
        {
            while (!task.IsCompleted) yield return null;
        }

        public static void ThrowExceptions(this Task task)
        {
            Debug.Assert(task.IsCompleted);
            task.GetAwaiter().GetResult();
        }

        public static T Expect<T>(this Task task)
            where T: Exception
        {
            try
            {
                task.ThrowExceptions();
            }
            catch (T ex)
            {
                Assert.IsNotNull(ex);
                return ex; // OK!
            }
            throw new AssertionException($"Expected task to throw {typeof(T)}");
        }
    }
}