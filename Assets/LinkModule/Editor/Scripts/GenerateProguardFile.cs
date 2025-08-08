#if UNITY_ANDROID
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LinkModule.Editor.Scripts
{
    public class GenerateProguardFile : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private readonly string[] _proguardRules =
        {
#if USE_FACEBOOK_SDK
            "########################################",
            "# Facebook SDK + Facebook Unity Plugin",
            "########################################",
            "-keep class com.facebook.unity.** { *; }",
            "-keep class com.facebook.** { *; }",
            "-dontwarn com.facebook.**",
            
            "########################################",
            "# Custom FB DeepLink Plugin",
            "########################################",
            "-keep class com.titan.fbdeeplinkplugin.** { *; }",
            "-dontwarn com.titan.fbdeeplinkplugin.**",
#endif

#if USE_APPSFLYER_SDK
            "########################################",
            "# AppsFlyer SDK",
            "########################################",
            "-keep class com.appsflyer.** { *; }",
            "-dontwarn com.appsflyer.**",
#endif

#if USE_FIREBASE_SDK
            "########################################",
            "# Firebase Analytics / Messaging / Remote Config",
            "########################################",
            "-keep class com.google.firebase.** { *; }",
            "-dontwarn com.google.firebase.**",
            "-keepattributes Signature",
            "-keepattributes *Annotation*",
#endif

            "########################################",
            "# Google Mobile Ads / AdMob",
            "########################################",
            "-keep class com.google.android.gms.ads.** { *; }",
            "-dontwarn com.google.android.gms.ads.**",
            "-keep class com.google.android.gms.tasks.** { *; }",
            "-dontwarn com.google.android.gms.tasks.**",

            "# Google Advertising ID",
            "-keep class com.google.android.gms.ads.identifier.** { *; }",
            "-dontwarn com.google.android.gms.ads.identifier.**",

            "########################################",
            "# Unity Ads",
            "########################################",
            "-keep class com.unity3d.ads.** { *; }",
            "-dontwarn com.unity3d.ads.**",

            "########################################",
            "# IronSource SDK",
            "########################################",
            "-keep class com.ironsource.** { *; }",
            "-dontwarn com.ironsource.**",

            "########################################",
            "# UniWebView (OneVcat)",
            "########################################",
            "-keep class com.onevcat.uniwebview.** { *; }",
            "-dontwarn com.onevcat.uniwebview.**",

            "########################################",
            "# Unity Player and Android Notifications",
            "########################################",
            "-keep class com.unity3d.player.** { *; }",
            "-keep class com.unity.androidnotifications.** { *; }",
            "-dontwarn com.unity3d.player.**",
            "-dontwarn com.unity.androidnotifications.**",
            "-keep class * extends com.unity3d.player.UnityPlayerActivity { *; }",

            "########################################",
            "# Android Support / AndroidX libraries",
            "########################################",
            "-keep class android.support.** { *; }",
            "-dontwarn android.support.**",
            "-keep class androidx.** { *; }",
            "-dontwarn androidx.**",

            "########################################",
            "# Titan Install Referrer Plugin",
            "########################################",
            "-keep class com.titan.referrerplugin.** { *; }",
            "-dontwarn com.titan.referrerplugin.**",

            "########################################",
            "# General Unity/Android Reflection Protection",
            "########################################",
            "-keepclassmembers class * { public <init>(...); }",
            "-keepclassmembers class * { public void *(...); }",
            "-keepclassmembers class * { public *; }"
        };

        public void OnPreprocessBuild(BuildReport report)
        {
            string dir = Path.Combine(Application.dataPath, "Plugins", "Android");
            string proguardPath = Path.Combine(dir, "proguard-user.txt");

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Debug.Log($"[ProguardGenerator] Created directory: {dir}");
            }

            var existingLines = File.Exists(proguardPath) 
                ? File.ReadAllLines(proguardPath).Select(line => line.Trim()).ToHashSet() 
                : new HashSet<string>();

            bool updated = false;

            using (var writer = new StreamWriter(proguardPath, append: true))
            {
                foreach (var rule in _proguardRules)
                {
                    if (!existingLines.Contains(rule.Trim()))
                    {
                        writer.WriteLine(rule);
                        updated = true;
                    }
                }
            }

            if (updated)
                Debug.Log("[ProguardGenerator] Proguard rules updated successfully.");
            else
                Debug.Log("[ProguardGenerator] Proguard rules already up to date.");
        }
    }
}
#endif