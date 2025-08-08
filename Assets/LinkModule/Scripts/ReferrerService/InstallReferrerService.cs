#if USE_INSTALL_REFERRER
using System;
using System.Collections.Generic;
using LinkModule.Scripts.Helper;
using UnityEngine;

namespace LinkModule.Scripts.ReferrerService
{
    public class InstallReferrerService : MonoBehaviour, IInstallReferrerService
    {
        public event Action<string> OnRawReferrerReceived;
        public event Action<Dictionary<string, string>> OnReferrerParsed;
        public event Action<int, string> OnFailed;
        
#if UNITY_ANDROID && !UNITY_EDITOR
        
        private static AndroidJavaObject CurrentActivity =>
            new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                .GetStatic<AndroidJavaObject>("currentActivity");
        
        public void Initialize()
        {
            Debug.Log("[InstallReferrer] Initializing...");
            try
            {
                new AndroidJavaClass("com.titan.referrerplugin.InstallReferrerHelper")
                    .CallStatic("getInstallReferrer", CurrentActivity, new ReferrerCallback(this));
            }
            catch (Exception e)
            {
                Debug.LogError("[InstallReferrerService] Initialization error: " + e.Message);
                OnFailed?.Invoke(-1, e.Message);
            }
        }
        
        [RunOnMainThread]
        public void ReferrerResult(int code, string rawReferrer)
        {
            if (!string.IsNullOrEmpty(rawReferrer) && code == 0)
            {
                Debug.Log("[InstallReferrer] Referrer success");
                OnRawReferrerReceived?.Invoke(rawReferrer);
                OnReferrerParsed?.Invoke(Parse(rawReferrer));
            }
            else
            {
                Debug.Log("[InstallReferrer] Referrer failed");
                OnFailed?.Invoke(code, rawReferrer);
            }
        }

        private static Dictionary<string, string> Parse(string raw)
        {
            var dict = new Dictionary<string, string>();
            
            if (string.IsNullOrEmpty(raw)) 
                return dict;

            var pairs = raw.Split('&');
            foreach (var pair in pairs)
            {
                var kv = pair.Split('=');
                
                if (kv.Length >= 2)
                {
                    var key = Uri.UnescapeDataString(kv[0]);
                    var value = Uri.UnescapeDataString(string.Join("=", kv, 1, kv.Length - 1));
                    dict[key] = value;
                }
            }

            return dict;
        }

        private class ReferrerCallback : AndroidJavaProxy
        {
            private readonly InstallReferrerService _service;

            public ReferrerCallback(InstallReferrerService service)
                : base("com.titan.referrerplugin.InstallReferrerHelper$ReferrerCallback")
            {
                _service = service;
            }

            void onReferrerResult(int code, string rawReferrer)
            {
                MainThreadInvoker.Invoke(_service, nameof(ReferrerResult), code, rawReferrer);
            }
        }
#else
        public void Initialize()
        {
            Debug.Log("[InstallReferrerService] Editor mock call.");

            string mockReferrer = "utm_source=google&utm_medium=cpc&utm_campaign=test&utm_content=editor-test";

            OnRawReferrerReceived?.Invoke(mockReferrer);
        }
#endif

    }
}
#endif