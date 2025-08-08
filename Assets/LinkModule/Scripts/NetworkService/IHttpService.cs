using System;
using System.Collections.Generic;

namespace LinkModule.Scripts.NetworkService
{
    public interface IHttpService
    {
        public void Post(string url, Dictionary<string, string> postData, Action<string> onSuccess,
            Action<string> onError);
        public void PostRaw(string url, string bodyText, Action<string> onSuccess, Action<string> onError);
        public void Get(string url, Action<string> onSuccess, Action<string> onError);
    }
}