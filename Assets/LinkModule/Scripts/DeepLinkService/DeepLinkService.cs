#if USE_FACEBOOK_SDK
using System;
using Facebook.Unity;
using Facebook.Unity.Settings;
using LinkModule.Scripts.Helper;
using UnityEngine;

namespace LinkModule.Scripts.DeepLinkService
{
    public class DeepLinkService : MonoBehaviour, IDeepLinkService
    {
        public event Action<string> OnDeepLinkReceived;
        public event Action<string> OnDeeplinkFailed;

        private const string JAVA_PLUGIN_CLASS = "com.titan.fbdeeplinkplugin.FacebookDeepLinkPlugin";
        private const string JAVA_CALLBACK_INTERFACE = JAVA_PLUGIN_CLASS + "$DeepLinkCallback";
        private const string UNITY_PLAYER_CLASS = "com.unity3d.player.UnityPlayer";
        private const string URI_PREFIX = "myapp://";

        public void Initialize()
        {
            Debug.Log("[DeepLink] Initializing Facebook SDK...");

            if (!FB.IsInitialized)
            {
                FB.Init(OnFacebookInitialized, OnHideUnity);
            }
            else
            {
                Debug.Log("[DeepLink] Facebook SDK already initialized.");
                FB.ActivateApp();
                FetchDeepLink();
            }
        }

        private void OnFacebookInitialized()
        {
            Debug.Log("[DeepLink] Facebook SDK initialized.");
            FB.ActivateApp();
            FetchDeepLink();
        }
        
        public void SetFacebookCredentials(string id, string token)
        {
#if UNITY_ANDROID
            int idx = FacebookSettings.SelectedAppIndex;

            while (FacebookSettings.AppIds.Count <= idx)
                FacebookSettings.AppIds.Add("");

            while (FacebookSettings.ClientTokens.Count <= idx)
                FacebookSettings.ClientTokens.Add("");

            FacebookSettings.AppIds[idx] = id;
            FacebookSettings.ClientTokens[idx] = token;
#endif
        }

        private void FetchDeepLink()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unityPlayer = new AndroidJavaClass(UNITY_PLAYER_CLASS);
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                using var plugin = new AndroidJavaClass(JAVA_PLUGIN_CLASS);
                plugin.CallStatic("getDeeps", activity, new DeepLinkCallback(this));
            }
            catch (Exception e)
            {
                Debug.LogError($"[DeepLink] Failed to call FB plugin: {e.Message}");
                OnDeeplinkFailed?.Invoke(e.Message);
            }
#else
            OnDeepLinkReceived?.Invoke("[DeepLink] Worked only on Android.");
#endif
        }

        private class DeepLinkCallback : AndroidJavaProxy
        {
            private readonly DeepLinkService _service;

            public DeepLinkCallback(DeepLinkService service)
                : base(JAVA_CALLBACK_INTERFACE)
            {
                _service = service;
            }

            void onDeepLinkResult(string result)
            {
                MainThreadInvoker.Invoke(_service, nameof(HandleDeepLink), result);
            }

            void onDeepLinkError(string error)
            {
                MainThreadInvoker.Invoke(_service, nameof(HandleDeepLinkError), error);
            }
        }

        [RunOnMainThread]
        private void HandleDeepLink(string url)
        {
            OnDeepLinkReceived?.Invoke(CleanUrl(url));
        }
        
        [RunOnMainThread]
        private void HandleDeepLinkError(string error)
        {
            OnDeeplinkFailed?.Invoke(error);
        }

        private string CleanUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            return url.StartsWith(URI_PREFIX) ? url.Substring(URI_PREFIX.Length) : url;
        }

        private void OnHideUnity(bool isGameShown)
        {
            Debug.Log($"[DeepLink] App visibility changed: {(isGameShown ? "shown" : "hidden")}");
        }
    }
}
#endif