using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinkModule.Scripts.Config;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LinkModule.Editor.Scripts
{
    [CustomEditor(typeof(ServicesConfig))]
    public class ServicesConfigEditor : UnityEditor.Editor
    {
        private static readonly string[] Syllables = 
        {
            "ra", "lo", "vi", "ka", "zu", "na", "mo", "ta", "ne", "ro",
            "li", "fa", "ze", "ki", "da", "so", "ma", "ti", "nu", "re"
        };

        private static readonly string[] Prefixes =
        {
            "neo", "tri", "vel", "syn", "gl", "pro", "ex", "hyper", "meta", "ultra"
        };

        private static readonly string[] Suffixes = 
        {
            "on", "ix", "ar", "us", "or", "ium", "ex", "os", "in", "en"
        };
        
        private class KeyDefinition
        {
            public readonly string DisplayName;
            public readonly Func<string> Getter;
            public readonly Action<string> Setter;

            public KeyDefinition(string displayName, Func<string> getter, Action<string> setter)
            {
                DisplayName = displayName;
                Getter = getter;
                Setter = setter;
            }
        }

        public override void OnInspectorGUI()
        {
            var config = (ServicesConfig)target;

            DrawTestMode(config);
            DrawGeneralConfig(config);
            DrawAppsFlyer(config);
            DrawKeyConfig(config);
            DrawGameConfig(config);
            DrawLoadingScene(config);

            if (GUI.changed)
                EditorUtility.SetDirty(config);
        }

        private static void DrawTestMode(ServicesConfig config)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Testing Mode", EditorStyles.boldLabel);
            config.isTest = EditorGUILayout.ToggleLeft("Test Mode Enabled", config.isTest);

            if (config.isTest)
            {
                EditorGUI.indentLevel++;
                config.runGame = EditorGUILayout.ToggleLeft("Run game in test mode", config.runGame);
                GUILayout.Space(5);
                config.testUrl = EditorGUILayout.TextField("Test URL", config.testUrl);

                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 15f);
                if (GUILayout.Button("Check URL", GUILayout.Width(150)))
                    if (!string.IsNullOrEmpty(config.testUrl))
                        Application.OpenURL(config.testUrl);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }
            
            GUILayout.Space(5);
            config.showLoadingState = EditorGUILayout.ToggleLeft("Show Loading State", config.showLoadingState);
        }

        private static void DrawGeneralConfig(ServicesConfig config)
        {
            GUILayout.Space(15);
            EditorGUILayout.LabelField("Release config", EditorStyles.boldLabel);
            config.domain = EditorGUILayout.TextField("Domain (Tracking URL)", config.domain);
            
#if USE_FACEBOOK_SDK
            GUILayout.Space(10);
            config.urlIdToken = EditorGUILayout.TextField("URL get Facebook Id, Token", config.urlIdToken);
#endif

            GUILayout.Space(10);
            config.decodeInstallReferrer = EditorGUILayout.ToggleLeft("Decode Install Referrer", config.decodeInstallReferrer);
            if (config.decodeInstallReferrer)
            {
                EditorGUI.indentLevel++;
                config.installReferrerDecodeUrl = EditorGUILayout.TextField("Referrer Decode URL", config.installReferrerDecodeUrl);
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawAppsFlyer(ServicesConfig config)
        {
            GUILayout.Space(15);
            EditorGUILayout.LabelField("AppsFlyer Config", EditorStyles.boldLabel);
            config.appsFlyerDevKey = EditorGUILayout.TextField("Dev Key", config.appsFlyerDevKey);
            config.appsFlyerAppId = EditorGUILayout.TextField("App ID (iOS)", config.appsFlyerAppId);
        }

        private static void DrawGameConfig(ServicesConfig config)
        {
            GUILayout.Space(15);
            EditorGUILayout.LabelField("Game Config", EditorStyles.boldLabel);
            config.enableSafeArea = EditorGUILayout.ToggleLeft("Enable safe area", config.enableSafeArea);
            config.orientationInGame = (ScreenOrientation)EditorGUILayout.EnumPopup("In-Game Orientation", config.orientationInGame);
            config.autorotateToLandscapeLeft = EditorGUILayout.ToggleLeft("Autorotate to Landscape Left", config.autorotateToLandscapeLeft);
            config.autorotateToLandscapeRight = EditorGUILayout.ToggleLeft("Autorotate to Landscape Right", config.autorotateToLandscapeRight);
            config.autorotateToPortrait = EditorGUILayout.ToggleLeft("Autorotate to Portrait", config.autorotateToPortrait);
            config.autorotateToPortraitUpsideDown = EditorGUILayout.ToggleLeft("Autorotate to Portrait Upside Down", config.autorotateToPortraitUpsideDown);
        }

        private static void DrawKeyConfig(ServicesConfig config)
        {
            GUILayout.Space(15);
            EditorGUILayout.LabelField("Randomize Keys in URL", EditorStyles.boldLabel);
            config.randomizedKeysInUrl = EditorGUILayout.ToggleLeft("Enable Randomization", config.randomizedKeysInUrl);

            if (!config.randomizedKeysInUrl) return;

            EditorGUI.indentLevel++;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Randomize All Keys"))
                RandomizeAllKeys(config);
            if (GUILayout.Button("Reset to Default"))
                ResetAllKeysToDefault(config);
            if (GUILayout.Button("Copy All Keys to Clipboard"))
                CopyAllKeysToClipboard(config);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            foreach (var key in GetActiveKeys(config))
            {
                key.Setter(EditorGUILayout.TextField(key.DisplayName, key.Getter()));
                GUILayout.Space(3);
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawLoadingScene(ServicesConfig config)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Loading Scene", EditorStyles.boldLabel);

            config.loadingScene =
                (SceneAsset)EditorGUILayout.ObjectField("Loading Scene", config.loadingScene, typeof(SceneAsset),
                    false);

            if (config.loadingScene is null) 
                return;
            
            if (GUILayout.Button("Add Scene to Build Settings (First)"))
            {
                var path = AssetDatabase.GetAssetPath(config.loadingScene);
                var scenes = EditorBuildSettings.scenes.ToList();
                scenes.RemoveAll(s => s.path == path);
                scenes.Insert(0, new EditorBuildSettingsScene(path, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        private static List<KeyDefinition> GetActiveKeys(ServicesConfig config)
        {
            var keys = new List<KeyDefinition>();

#if USE_APPSFLYER_SDK || USE_FACEBOOK_SDK || USE_INSTALL_REFERRER
            keys.Add(new KeyDefinition("sub_id_1", () => config.sub_id_1_Key, v => config.sub_id_1_Key = v));
#endif
#if USE_FACEBOOK_SDK
            keys.Add(new KeyDefinition("ad_id", () => config.ad_id_Key, v => config.ad_id_Key = v));
#endif
#if USE_FIREBASE_SDK
            keys.Add(new KeyDefinition("push_token", () => config.push_token_Key, v => config.push_token_Key = v));
#endif
#if USE_APPSFLYER_SDK
            keys.Add(new KeyDefinition("deviceID", () => config.deviceID_Key, v => config.deviceID_Key = v));
#endif
#if USE_INSTALL_REFERRER
			if (config.decodeInstallReferrer)
			{
                keys.Add(new KeyDefinition("campaign_group_name_ref", () => config.campaign_group_name_ref_Key,
                    v => config.campaign_group_name_ref_Key = v));
                keys.Add(new KeyDefinition("ad_id_ref", () => config.ad_id_ref_Key, v => config.ad_id_ref_Key = v));
                keys.Add(new KeyDefinition("ad_objective_name_ref", () => config.ad_objective_name_ref_Key,
                    v => config.ad_objective_name_ref_Key = v));
                keys.Add(new KeyDefinition("adgroup_id_ref", () => config.adgroup_id_ref_Key,
                    v => config.adgroup_id_ref_Key = v));
                keys.Add(new KeyDefinition("adgroup_name_ref", () => config.adgroup_name_ref_Key,
                    v => config.adgroup_name_ref_Key = v));
                keys.Add(new KeyDefinition("campaign_id_ref", () => config.campaign_id_ref_Key,
                    v => config.campaign_id_ref_Key = v));
                keys.Add(new KeyDefinition("campaign_name_ref", () => config.campaign_name_ref_Key,
                    v => config.campaign_name_ref_Key = v));
                keys.Add(new KeyDefinition("campaign_group_id_ref", () => config.campaign_group_id_ref_Key,
                    v => config.campaign_group_id_ref_Key = v));
                keys.Add(new KeyDefinition("account_id_ref", () => config.account_id_ref_Key,
                    v => config.account_id_ref_Key = v));
                keys.Add(new KeyDefinition("is_instagram_ref", () => config.is_instagram_ref_Key,
                    v => config.is_instagram_ref_Key = v));
                keys.Add(new KeyDefinition("is_an_ref", () => config.is_an_ref_Key, v => config.is_an_ref_Key = v));
                keys.Add(new KeyDefinition("publisher_platform_ref", () => config.publisher_platform_ref_Key,
                    v => config.publisher_platform_ref_Key = v));
                keys.Add(new KeyDefinition("platform_position_ref", () => config.platform_position_ref_Key,
                    v => config.platform_position_ref_Key = v));
			}
            else
            {
                keys.Add(new KeyDefinition("naming", () => config.naming_Key, v => config.naming_Key = v));
            }
#endif
            return keys;
        }

        private static void RandomizeAllKeys(ServicesConfig config)
        {
            var used = new HashSet<string>();

            foreach (var key in GetActiveKeys(config))
                key.Setter(Unique());
            return;

            string Unique() 
            {
                string k;
                do
                {
                    k = RandomString(Random.Range(4, 10));
                } 
                while (!used.Add(k));
                return k;
            }
        }

        private static void ResetAllKeysToDefault(ServicesConfig config)
        {
            foreach (var key in GetActiveKeys(config))
                key.Setter(key.DisplayName);
        }

        private static void CopyAllKeysToClipboard(ServicesConfig config)
        {
            var sb = new StringBuilder();
            foreach (var key in GetActiveKeys(config))
                sb.AppendLine($"{key.DisplayName} -> {key.Getter()}");

            EditorGUIUtility.systemCopyBuffer = sb.ToString();
            EditorUtility.DisplayDialog("Copied", "All current keys have been copied to clipboard.", "OK");
        }

        private static string RandomString(int length)
        {
            var sb = new StringBuilder();

            if (Random.value > 0.7f)
                sb.Append(Prefixes[Random.Range(0, Prefixes.Length)]);

            while (sb.Length < length)
            {
                string syllable = Syllables[Random.Range(0, Syllables.Length)];
                if (sb.Length + syllable.Length > length)
                    break;
                sb.Append(syllable);
            }

            if (Random.value > 0.6f && sb.Length < length + 4)
                sb.Append(Suffixes[Random.Range(0, Suffixes.Length)]);

            while (sb.Length < length)
                sb.Append((char)Random.Range('a', 'z' + 1));

            return sb.ToString();
        }
    }
}