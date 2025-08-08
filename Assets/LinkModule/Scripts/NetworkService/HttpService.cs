using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LinkModule.Scripts.NetworkService
{
    public class HttpService : MonoBehaviour, IHttpService
    {
        private const int MAX_RETRIES = 3;
        private const float RETRY_DELAY_SECONDS = 2f;

        public void Post(string url, Dictionary<string, string> postData, Action<string> onSuccess, Action<string> onError)
        {
            StartCoroutine(SendWithRetry(() =>
            {
                var form = new WWWForm();
                foreach (var pair in postData)
                {
                    form.AddField(pair.Key, pair.Value);
                }
                return UnityWebRequest.Post(url, form);
            }, onSuccess, onError));
        }

        public void PostRaw(string url, string bodyText, Action<string> onSuccess, Action<string> onError)
        {
            StartCoroutine(SendWithRetry(() =>
            {
                var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                var bodyRaw = System.Text.Encoding.UTF8.GetBytes(bodyText);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "text/plain; charset=UTF-8");
                return request;
            }, onSuccess, onError));
        }

        public void Get(string url, Action<string> onSuccess, Action<string> onError)
        {
            StartCoroutine(SendWithRetry(() => UnityWebRequest.Get(url), onSuccess, onError));
        }

        private IEnumerator SendWithRetry(Func<UnityWebRequest> requestFactory, Action<string> onSuccess, Action<string> onError)
        {
            if (requestFactory == null)
            {
                onError?.Invoke("[HttpService] Request factory is null.");
                yield break;
            }

            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                UnityWebRequest www;

                try
                {
                    www = requestFactory();
                    if (www == null)
                    {
                        onError?.Invoke("[HttpService] Request factory returned null.");
                        yield break;
                    }
                }
                catch (Exception e)
                {
                    onError?.Invoke($"[HttpService] Failed to create request: {e.Message}");
                    yield break;
                }

                using (www)
                {
                    www.timeout = 10;
                    yield return www.SendWebRequest();

                    bool success = www.result == UnityWebRequest.Result.Success;

                    if (success)
                    {
                        onSuccess?.Invoke(www.downloadHandler.text);
                        yield break;
                    }

                    Debug.LogWarning($"[HttpService] Attempt {attempt} failed: {www.error}");

                    if (attempt == MAX_RETRIES)
                    {
                        onError?.Invoke($"Request to {www.url} failed after {MAX_RETRIES} attempts: {www.error}");
                        yield break;
                    }

                    yield return new WaitForSeconds(RETRY_DELAY_SECONDS);
                }
            }
        }
    }
}
