using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace SimpleGraphQL
{
    public static class Utils
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Checks if a string is null, empty, or contains whitespace.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrWhitespace(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                foreach (char c in str)
                {
                    if (!char.IsWhiteSpace(c))
                        return false;
                }
            }

            return true;
        }

        public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            return new UnityWebRequestAwaiter(asyncOp);
        }

        public class UnityWebRequestAwaiter : INotifyCompletion
        {
            private UnityWebRequestAsyncOperation asyncOp;
            private Action continuation;

            public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
            {
                this.asyncOp = asyncOp;
                asyncOp.completed += OnRequestCompleted;
            }

            public bool IsCompleted
            {
                get { return asyncOp.isDone; }
            }

            public void GetResult() { }

            public void OnCompleted(Action continuation)
            {
                this.continuation = continuation;
            }

            private void OnRequestCompleted(AsyncOperation obj)
            {
                continuation();
            }
        }
    }
}