using System.IO;
using LinkModule.Scripts.Config;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LinkModule.Editor.Scripts
{
    public class BuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var config = Resources.Load<ServicesConfig>("PluginConfig/ServicesConfig");
            
            if (config is null)
            {
                Debug.LogWarning("⚠️ServicesConfig not found in Resources.");
                return;
            }

            var isDevelopment = EditorUserBuildSettings.development;

            if (!isDevelopment)
            {
                CheckTestMode(config);
                CheckEmptyDomain(config);
            }

#if USE_FIREBASE_SDK
            CheckFirebaseFile();
#endif

#if USE_APPSFLYER_SDK
            CheckAppsFlyer(config);
#endif
        }

        private void CheckTestMode(ServicesConfig config)
        {
            if (config.isTest)
                ShowCancelableWarning(
                    "⚠️ Test mode is enabled",
                    "Test mode is enabled during release build.\n\n" +
                    "To disable it, go to ServicesConfig:\nAssets/LinkModule/Resources/ServicesConfig"
                );
        }

        private void CheckEmptyDomain(ServicesConfig config)
        {
            if (string.IsNullOrEmpty(config.domain))
                ShowCancelableWarning(
                    "⚠️ Release domain is empty",
                    "A domain is required for release builds.\n\n" +
                    "Go to ServicesConfig:\nAssets/LinkModule/Resources/ServicesConfig"
                );
        }

#if USE_FIREBASE_SDK
        private void CheckFirebaseFile()
        {
            var firebasePath = Path.Combine(Application.dataPath, "google-services.json");
            if (!File.Exists(firebasePath))
                ShowCancelableWarning(
                    "⚠️ Missing google-services.json",
                    "File `google-services.json` not found in Assets folder.\n\nFirebase may not work properly!"
                );
        }
#endif

#if USE_APPSFLYER_SDK
        private void CheckAppsFlyer(ServicesConfig config)
        {
#if UNITY_IOS
        if (string.IsNullOrEmpty(config.appsFlyerDevKey) || string.IsNullOrEmpty(config.appsFlyerAppId))
        {
            ShowCancelableWarning(
                "⚠️ AppsFlyer Dev Key and App ID required (iOS)",
                "Both Dev Key and App ID must be set in ServicesConfig."
            );
        }
#endif

            if (string.IsNullOrEmpty(config.appsFlyerDevKey))
                ShowCancelableWarning(
                    "⚠️ AppsFlyer Dev Key is required",
                    "Dev Key is empty in ServicesConfig. AppsFlyer won't function correctly."
                );
        }
#endif

        private void ShowCancelableWarning(string title, string message)
        {
            var continueBuild = EditorUtility.DisplayDialog(
                title,
                message + "\n\nDo you want to continue the build?",
                "Continue Build",
                "Cancel Build"
            );

            if (!continueBuild) 
                throw new BuildFailedException($"Build canceled: {title}");
        }
    }
}