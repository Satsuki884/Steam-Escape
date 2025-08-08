using UnityEditor;
using UnityEngine;

namespace LinkModule.Scripts.Config
{
    [CreateAssetMenu(fileName = "ServicesConfig", menuName = "Config/ServicesConfig", order = 1)]
    public class ServicesConfig : ScriptableObject
    {
        public bool isTest;
        public bool showLoadingState;
        public bool runGame;
        public string testUrl;
        public bool decodeInstallReferrer;
        public string installReferrerDecodeUrl;
#if USE_FACEBOOK_SDK
        public string urlIdToken;
#endif
        public string domain;
        public string appsFlyerDevKey;
        public string appsFlyerAppId;
        public bool enableSafeArea = true;
        public ScreenOrientation orientationInGame;
        public bool autorotateToLandscapeLeft,
            autorotateToLandscapeRight,
            autorotateToPortrait,
            autorotateToPortraitUpsideDown;
        
        public bool randomizedKeysInUrl;
#if USE_APPSFLYER_SDK || USE_FACEBOOK_SDK || USE_INSTALL_REFERRER
        public string sub_id_1_Key = "sub_id_1";
#endif
            
#if USE_FACEBOOK_SDK || USE_APPSFLYER_SDK
        public string ad_id_Key = "ad_id";
#endif
        
#if USE_FIREBASE_SDK
        public string push_token_Key = "push_token";
#endif
        
#if USE_APPSFLYER_SDK
        public string deviceID_Key = "deviceID";
#endif
        
#if USE_INSTALL_REFERRER
        public string campaign_group_name_ref_Key = "campaign_group_name_ref";
        public string ad_id_ref_Key = "ad_id_ref";
        public string ad_objective_name_ref_Key = "ad_objective_name_ref";
        public string adgroup_id_ref_Key = "adgroup_id_ref";
        public string adgroup_name_ref_Key = "adgroup_name_ref";
        public string campaign_id_ref_Key = "campaign_id_ref";
        public string campaign_name_ref_Key = "campaign_name_ref";
        public string campaign_group_id_ref_Key = "campaign_group_id_ref";
        public string account_id_ref_Key = "account_id_ref";
        public string is_instagram_ref_Key = "is_instagram_ref";
        public string is_an_ref_Key = "is_an_ref";
        public string publisher_platform_ref_Key = "publisher_platform_ref";
        public string platform_position_ref_Key = "platform_position_ref";
        public string naming_Key = "naming";
#endif
        
#if UNITY_EDITOR
        public SceneAsset loadingScene;
#endif
    }
}