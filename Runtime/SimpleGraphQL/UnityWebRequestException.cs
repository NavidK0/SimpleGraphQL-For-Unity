// Based on: https://github.com/Cysharp/UniTask/blob/master/src/UniTask/Assets/Plugins/UniTask/Runtime/UnityWebRequestException.cs

// The MIT License (MIT)
//
// Copyright (c) 2019 Yoshifumi Kawai / Cysharp, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Networking;

namespace SimpleGraphQL
{
    [PublicAPI]
    public class UnityWebRequestException : Exception
    {
        public UnityWebRequest UnityWebRequest { get; }
#if UNITY_2020_2_OR_NEWER
        public UnityWebRequest.Result Result { get; }
#else
        public bool IsNetworkError { get; }
        public bool IsHttpError { get; }
#endif
        public string Error { get; }
        public string Text { get; }
        public long ResponseCode { get; }
        public Dictionary<string, string> ResponseHeaders { get; }

        private string _msg;

        public UnityWebRequestException(UnityWebRequest unityWebRequest)
        {
            UnityWebRequest = unityWebRequest;
#if UNITY_2020_2_OR_NEWER
            Result = unityWebRequest.result;
#else
            this.IsNetworkError = unityWebRequest.isNetworkError;
            this.IsHttpError = unityWebRequest.isHttpError;
#endif
            Error = unityWebRequest.error;
            ResponseCode = unityWebRequest.responseCode;

            if (UnityWebRequest.downloadHandler != null)
            {
                if (unityWebRequest.downloadHandler is DownloadHandlerBuffer dhb)
                {
                    Text = dhb.text;
                }
            }

            ResponseHeaders = unityWebRequest.GetResponseHeaders();
        }

        public override string Message
        {
            get
            {
                if (_msg == null)
                {
                    if (Text != null)
                    {
                        _msg = Error + Environment.NewLine + Text;
                    }
                    else
                    {
                        _msg = Error;
                    }
                }

                return _msg;
            }
        }
    }
}