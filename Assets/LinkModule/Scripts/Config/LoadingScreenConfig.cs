using UnityEngine;

namespace LinkModule.Scripts.Config
{
    [CreateAssetMenu(fileName = "LoadingScreenConfig", menuName = "Config/LoadingScreenConfig", order = 1)]
    public class LoadingScreenConfig : ScriptableObject
    {
        public Sprite loadingSprite;
        public Color startProgressBarColor;
        public Color endProgressBarColor;
        public Color backgroundProgressBarColor;
        public Color glowProgressBarColor;
    }
}