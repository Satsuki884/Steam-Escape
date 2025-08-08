#if USE_APPSFLYER_SDK
using System;
using AppsFlyerSDK;
using LinkModule.Scripts.Config;
using UnityEngine;

namespace LinkModule.Scripts.DeepLinkService
{

    public class AppsFlyerService : MonoBehaviour, IAppsFlyerConversionData, IAppsFlyerService, IConfigurable<ServicesConfig>
    {
        public event Action<string> OnCampaignReceived;
        public event Action<string> OnCampaignFailed;
        
        private ServicesConfig _config;
        
        public string DeviceId => AppsFlyer.getAppsFlyerId();
        
        public void SetConfig(ServicesConfig config)
        {
            _config = config;
        }
        
        public void Initialize()
        {
#if UNITY_IOS
            AppsFlyer.setIsDebug(_config.isTest);
            AppsFlyer.initSDK(_config.appsFlyerDevKey, _config.appsFlyerAppId, this);
            AppsFlyer.startSDK();
#elif UNITY_ANDROID
            AppsFlyer.setIsDebug(_config.isTest);
            AppsFlyer.initSDK(_config.appsFlyerDevKey, "", this);
            AppsFlyer.startSDK();
#endif
            Debug.Log("[AppsFlyer] AppsFlyer SDK initialized.");
        }

        public void onConversionDataSuccess(string conversionData)
        {
            AppsFlyer.AFLog("[AppsFlyer] onConversionDataSuccess", conversionData);
            var data = AppsFlyer.CallbackStringToDictionary(conversionData);

            if (data.TryGetValue("campaign", out var value))
            {
                OnCampaignReceived?.Invoke(value.ToString());
            }
            else
            {
                OnCampaignFailed?.Invoke(conversionData);
            }
        }

        public void onConversionDataFail(string error)
        {
            AppsFlyer.AFLog("[AppsFlyer] onConversionDataFail", error);
            OnCampaignFailed?.Invoke(error);
        }

        public void onAppOpenAttribution(string attributionData)
        {
            AppsFlyer.AFLog("[AppsFlyer] onAppOpenAttribution", "Call is ignored.");
        }

        public void onAppOpenAttributionFailure(string error)
        {
            AppsFlyer.AFLog("[AppsFlyer] onAppOpenAttributionFailure", "Call is ignored.");
        }
    }
}
#endif