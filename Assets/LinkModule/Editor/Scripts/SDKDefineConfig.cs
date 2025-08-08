using System.Collections.Generic;
using UnityEngine;

namespace LinkModule.Editor.Scripts
{
    [CreateAssetMenu(fileName = "SDKDefineConfig", menuName = "Config/SDK Define Config")]
    public class SDKDefineConfig : ScriptableObject
    {
        public List<SDKDefineEntry> sdkList = new();
    }
    
    [System.Serializable]
    public class SDKDefineEntry
    {
        [Tooltip("The scripting define symbol that will be added to Player Settings if the SDK is enabled.")]
        public string defineSymbol;

        [Tooltip("The display name of the SDK shown in the editor window.")]
        public string displayName;

        [Tooltip("The full name (namespace + class) of a type to check if the SDK is present in the project.")]
        public string typeNameToCheck;

        [Tooltip("Optional assembly name where the type is located (used for more precise type lookup). Leave empty if not needed.")]
        public string optionalAssemblyName;
    }
}