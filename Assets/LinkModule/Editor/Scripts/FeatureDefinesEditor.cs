using System;
using System.Collections.Generic;
using LinkModule.Scripts.Config;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace LinkModule.Editor.Scripts
{
    public class FeatureDefinesEditor : EditorWindow
    {
        private class SDKInfo
        {
            public string DefineSymbol;
            public string DisplayName;
            public Func<bool> IsAvailable;
            public bool Enabled;
        }

        private static readonly BuildTargetGroup[] SupportedGroups =
        {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS
        };

        private static readonly string[] SupportedGroupNames =
        {
            "Standalone", "Android", "iOS"
        };

        private static readonly Dictionary<string, Type> TypeCache = new();
        private readonly List<SDKInfo> _sdkList = new();
        private bool _hasChanges;
        private BuildTargetGroup _buildTargetGroup = BuildTargetGroup.Android;

        [MenuItem("Tools/Service Configs")]
        public static void ShowWindow()
        {
            GetWindow<FeatureDefinesEditor>("Service Configs");
        }

        private void OnEnable()
        {
            InitSDKList();
            LoadCurrentDefines();
        }

        private void InitSDKList()
        {
            _sdkList.Clear();
            TypeCache.Clear();

            var config = Resources.Load<SDKDefineConfig>("Configs/SDKDefineConfig");

            if (config == null)
            {
                Debug.LogError("SDKDefineConfig not found in Resources!");
                return;
            }

            foreach (var entry in config.sdkList)
            {
                _sdkList.Add(new SDKInfo
                {
                    DefineSymbol = entry.defineSymbol,
                    DisplayName = entry.displayName,
                    IsAvailable = string.IsNullOrEmpty(entry.typeNameToCheck)
                        ? () => true
                        : () => TryFindType(entry.typeNameToCheck, entry.optionalAssemblyName) != null
                });
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Service Configs", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawConfigButton<ServicesConfig>("PluginConfig/ServicesConfig", "Open Config");
            DrawConfigButton<ScriptableObject>("LoadingScreen/LoadingScreenConfig", "Open Loading Screen Config");

            EditorGUILayout.Space(10);
            int currentIndex = Array.IndexOf(SupportedGroups, _buildTargetGroup);
            if (currentIndex < 0) 
                currentIndex = 0;

            int selectedIndex = EditorGUILayout.Popup("Build Target Group", currentIndex, SupportedGroupNames);
            _buildTargetGroup = SupportedGroups[selectedIndex];

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Available SDKs", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("↻", GUILayout.Width(24), GUILayout.Height(18)))
            {
                InitSDKList();
                LoadCurrentDefines();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            foreach (var sdk in _sdkList)
            {
                bool available = sdk.IsAvailable();
                string tooltip = available ? "SDK or feature is available" : "Missing dependency (type or assembly)";
                GUIContent labelContent = new GUIContent(sdk.DisplayName, tooltip);

                EditorGUI.BeginDisabledGroup(!available);
                EditorGUILayout.BeginHorizontal();

                bool previous = sdk.Enabled;
                sdk.Enabled = EditorGUILayout.ToggleLeft(labelContent, sdk.Enabled);
                if (sdk.Enabled != previous) _hasChanges = true;

                GUIStyle statusStyle = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = available ? Color.green : Color.red },
                    fontStyle = FontStyle.Bold
                };
                GUILayout.Label(available ? "✔ Available" : "✖ Not found", statusStyle, GUILayout.Width(100));

                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(!_hasChanges);
            if (GUILayout.Button("Save"))
            {
                ApplyDefines();
                _hasChanges = false;
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawConfigButton<T>(string path, string buttonLabel) where T : UnityEngine.Object
        {
            if (GUILayout.Button(buttonLabel))
            {
                var config = Resources.Load<T>(path);
                if (config != null)
                {
                    Selection.activeObject = config;
                    EditorGUIUtility.PingObject(config);
                }
                else
                {
                    EditorUtility.DisplayDialog("Config not found", $"Could not find config at Resources/{path}", "OK");
                }
            }
        }

        private static Type TryFindType(string fullTypeName, string optionalAssembly = null)
        {
            string key = optionalAssembly != null ? $"{fullTypeName}, {optionalAssembly}" : fullTypeName;

            if (TypeCache.TryGetValue(key, out var cached))
                return cached;

            Type type = Type.GetType(key);
            if (type == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        type = assembly.GetType(fullTypeName);
                        if (type != null) break;
                    }
                    catch
                    {
                        Debug.LogWarning($"Failed to load type {fullTypeName} from assembly {assembly.FullName}");
                    }
                }
            }

            TypeCache[key] = type;
            return type;
        }

        private void LoadCurrentDefines()
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(_buildTargetGroup);
            var defineSet = new HashSet<string>(defines.Split(';'));

            foreach (var sdk in _sdkList)
            {
                sdk.Enabled = defineSet.Contains(sdk.DefineSymbol);
            }

            _hasChanges = false;
        }

        private void ApplyDefines()
        {
            var newDefines = new List<string>();
            foreach (var sdk in _sdkList)
            {
                if (sdk.Enabled && sdk.IsAvailable())
                    newDefines.Add(sdk.DefineSymbol);
            }

            string defineString = string.Join(";", newDefines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(_buildTargetGroup, defineString);

            Debug.Log("Updated Scripting Define Symbols:\n" + defineString);
            AssetDatabase.Refresh();
            CompilationPipeline.RequestScriptCompilation();
        }
    }
}