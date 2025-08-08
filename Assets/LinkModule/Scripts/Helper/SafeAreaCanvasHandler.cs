using LinkModule.Scripts.Config;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LinkModule.Scripts.Helper
{
    [DefaultExecutionOrder(-1000)]
    public class SafeAreaCanvasHandler : MonoBehaviour
    {
        [SerializeField] private ServicesConfig config;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

#if UNITY_6000_0_OR_NEWER
            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) 
#else
            foreach (var canvas in FindObjectsOfType<Canvas>()) 
#endif
                ApplySafeAreaToCanvas(canvas);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private void OnRuntimeMethodLoad()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
#if UNITY_6000_0_OR_NEWER
            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) 
#else
            foreach (var canvas in FindObjectsOfType<Canvas>()) 
#endif
                ApplySafeAreaToCanvas(canvas);
        }

        private void ApplySafeAreaToCanvas(Canvas canvas)
        {
            if (!config.enableSafeArea)
                return;
            
            if (!canvas.isRootCanvas)
                return;

            // Check if already added
            var existing = canvas.transform.Find("SafeArea");
            if (existing is not null)
                return;

            // Create wrapper object inside Canvas
            var safeAreaGO = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
            var safeRect = safeAreaGO.GetComponent<RectTransform>();
            safeAreaGO.transform.SetParent(canvas.transform, false);

            // Fit full stretch
            safeRect.anchorMin = Vector2.zero;
            safeRect.anchorMax = Vector2.one;
            safeRect.offsetMin = Vector2.zero;
            safeRect.offsetMax = Vector2.zero;

            // Move all root UI objects under SafeArea
            var children = new Transform[canvas.transform.childCount];
            for (var i = 0; i < children.Length; i++)
                children[i] = canvas.transform.GetChild(i);

            foreach (var child in children)
            {
                if (child.name == "SafeArea")
                    continue;
                child.SetParent(safeAreaGO.transform, false);
            }

            Debug.Log($"[SafeAreaCanvasHandler] Applying safe area to canvas: {canvas.name}");
        }
    }
}