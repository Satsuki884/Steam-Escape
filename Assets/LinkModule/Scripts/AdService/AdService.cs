using System;
using System.Collections;
using UnityEngine;

namespace LinkModule.Scripts.AdService
{
    public class AdService : MonoBehaviour, IAdService
    {
        public event Action<string> OnAdIdReceived;
        
        private string _adId = string.Empty;
        private Coroutine _adIdCoroutine;
        
        public void Initialize()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _adIdCoroutine = StartCoroutine(RequestAdIdAndroid());
#elif UNITY_IOS || UNITY_EDITOR
            _adIdCoroutine = StartCoroutine(RequestAdIdIos());
#else
        Debug.LogWarning("AdService: Platform not supported for retrieving advertising ID.");
#endif
        }
    
#if UNITY_ANDROID && !UNITY_EDITOR
    private IEnumerator RequestAdIdAndroid()
    {
        string adId = string.Empty;
        try
        {
            using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using AndroidJavaClass advertisingIdClient = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
            AndroidJavaObject adInfo = advertisingIdClient.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", currentActivity);
            adId = adInfo.Call<string>("getId");
            bool isLimitAdTrackingEnabled = adInfo.Call<bool>("isLimitAdTrackingEnabled");
            Debug.Log("Android Advertising ID: " + adId + ", Limit Ad Tracking: " + isLimitAdTrackingEnabled);
        }
        catch (Exception e)
        {
            Debug.LogError("Error retrieving Android Advertising ID: " + e.Message);
        }
        yield return null;
        _adId = adId;
        OnAdIdReceived?.Invoke(adId);
    }
#endif

#if UNITY_IOS || UNITY_EDITOR
        private IEnumerator RequestAdIdIos()
        {
            bool callbackCalled = false;
            string adId = string.Empty;
            bool trackingEnabled = false;
            string error = string.Empty;
            float timeout = 10f;
            float elapsedTime = 0f;

            Application.RequestAdvertisingIdentifierAsync((id, trackEnabled, err) =>
            {
                adId = id;
                trackingEnabled = trackEnabled;
                error = err;
                callbackCalled = true;
            });

            while (!callbackCalled && elapsedTime < timeout)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (!callbackCalled)
            {
                Debug.LogError("Timeout while retrieving Advertising ID.");
            }
            else if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError("Error retrieving advertising ID: " + error);
            }
            else
            {
                Debug.Log("Advertising ID: " + adId + " Tracking Enabled: " + trackingEnabled);
            }
            
            _adId = adId;
            OnAdIdReceived?.Invoke(adId);
        }
#endif

        public string GetAdId()
        {
            if (!string.IsNullOrEmpty(_adId)) 
                return _adId;
            
            Debug.LogWarning("AdService: no Ad ID specified");
            return string.Empty;

        }
        
        private void OnDestroy()
        {
            if (_adIdCoroutine != null)
            {
                StopCoroutine(_adIdCoroutine);
            }
        }
    }
}