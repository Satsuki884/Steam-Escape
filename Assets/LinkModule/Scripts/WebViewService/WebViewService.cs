#if USE_UNIWEBVIEW
using System;
using LinkModule.Scripts.Config;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

namespace LinkModule.Scripts.WebViewService
{
    public sealed class WebViewService : MonoBehaviour, IWebViewService, IConfigurable<ServicesConfig>
    {
        public event Action OnWebViewClose;

        public void SetConfig(ServicesConfig _) { }

        public void Initialize() => CreateView();

        public bool IsWebViewOpened() => _webView is not null;

        public void LoadUrl(string url)
        {
            if (_webView is null)
                CreateView();
            
            _webView.Load(url);
            _webView.Show();
        }

        private readonly Color _backgroundColor = Color.black;
        private Canvas _canvas;
        private GameObject _bgGO, _viewGO;
        private UniWebView _webView;
        private float _gestureBar = -1f;
        private bool _waitingForPermission;

        private static readonly string[] Schemes = { "tg", "whatsapp", "viber", "diia", "game" };
        private const float KEYBOARD_THRESHOLD = .20f;

        private void Update()
        {
#if UNITY_IOS || UNITY_ANDROID
            if (_webView is null) 
                return;

            float keyboardHeight = Mathf.Round(GetKeyboardHeight());
            float minKeyboardThreshold = Screen.height * KEYBOARD_THRESHOLD;

            Rect safe = Screen.safeArea;
            float top = Screen.height - safe.yMax;
            float webViewHeight = safe.height;

            if (keyboardHeight > minKeyboardThreshold)
                webViewHeight -= keyboardHeight - GetGestureBarHeight();

            _webView.Frame = new Rect(safe.x, top, safe.width, webViewHeight);
#endif

#if UNITY_ANDROID
            if (_waitingForPermission)
            {
                _waitingForPermission = false;
                Permission.RequestUserPermission(Permission.Camera);
                _webView.Reload();
            }
#endif
        }

        private void OnDestroy() => Cleanup();

        private void CreateView()
        {
            Cleanup();
#if UNITY_6000_0_OR_NEWER
            _canvas ??= FindFirstObjectByType<Canvas>();
#else
            _canvas ??= FindObjectOfType<Canvas>();
#endif
            if (_canvas is null)
            {
                Debug.LogError("WebViewService: No Canvas found.");
                return;
            }

            _bgGO = new GameObject("WebView_BG", typeof(Image));
            _bgGO.transform.SetParent(_canvas.transform, false);
            _bgGO.transform.SetAsLastSibling();
            var bgRect = (RectTransform)_bgGO.transform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.one * -800f;
            bgRect.offsetMax = Vector2.one * 800f;
            _bgGO.GetComponent<Image>().color = _backgroundColor;

            _viewGO = new GameObject("WebView_Container", typeof(RectTransform));
            _viewGO.transform.SetParent(_canvas.transform, false);
            _viewGO.transform.SetAsLastSibling();
            ((RectTransform)_viewGO.transform).anchorMin = Vector2.zero;
            ((RectTransform)_viewGO.transform).anchorMax = Vector2.zero;

            _webView = _viewGO.AddComponent<UniWebView>();
            ConfigureWebView();
        }

        private void ConfigureWebView()
        {
            UniWebView.SetJavaScriptEnabled(true);
#if UNITY_IOS
            _webView.EmbeddedToolbar.SetPosition(UniWebViewToolbarPosition.Bottom);
            _webView.EmbeddedToolbar.Show();
#endif
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.orientation = ScreenOrientation.AutoRotation;

            _webView.OnShouldClose += OnShouldClose;
            _webView.OnMessageReceived += OnMessage;
            _webView.OnLoadingErrorReceived += OnErrorReceived;
            
            _webView.RegisterOnRequestMediaCapturePermission(OnRequestMediaCapturePermission);

            foreach (string s in Schemes)
                _webView.AddUrlScheme(s);

            string defaultAgent = _webView.GetUserAgent();
            string customAgent = defaultAgent.Replace("; wv", "").Replace("Version/4.0", "").Trim();
            _webView.SetUserAgent(customAgent);
            _webView.OnPageFinished += OnPageFinished;
        }

        private UniWebViewMediaCapturePermissionDecision OnRequestMediaCapturePermission(UniWebViewChannelMethodMediaCapturePermission permission)
        {
#if UNITY_ANDROID
            if (Permission.HasUserAuthorizedPermission(Permission.Camera))
                return UniWebViewMediaCapturePermissionDecision.Grant;
            
            _waitingForPermission = true;
            return UniWebViewMediaCapturePermissionDecision.Deny;
#else
            return UniWebViewMediaCapturePermissionDecision.Grant;
#endif
        }
        
        private void OnPageFinished(UniWebView view, int statusCode, string url)
        {
#if UNITY_ANDROID
            if (Permission.HasUserAuthorizedPermission(Permission.Camera))
                return;
            
            string js = @"
                (function() {
                    var inputs = Array.from(document.querySelectorAll('input[type=file]'));
                    return inputs.some(i =>
                        i.hasAttribute('capture') ||
                        (i.accept && (i.accept.includes('image') || i.accept.includes('camera')))
                    );
                })();
            ";
            
            view.EvaluateJavaScript(js, (payload) =>
            {
                if (payload.resultCode == "0" && payload.data == "true")
                {
                    Permission.RequestUserPermission(Permission.Camera);
                }
            });
#endif
        }
        
        private void OnMessage(UniWebView v, UniWebViewMessage m)
        {
            if (!IsCustomScheme(m.RawMessage)) return;

            if (m.RawMessage.StartsWith("game://", StringComparison.Ordinal))
            {
                _webView.Hide(false, UniWebViewTransitionEdge.None, 0f, () =>
                {
                    OnWebViewClose?.Invoke();
                    Destroy(gameObject);
                });
            }
            else
            {
                TryOpenExternalApp(m.RawMessage);
            }
        }

        private static bool IsCustomScheme(string url)
        {
            foreach (string s in Schemes)
                if (url.StartsWith(s + "://", StringComparison.Ordinal))
                    return true;
            return false;
        }
        
        private void OnErrorReceived(UniWebView webView, int errorCode, string errorMessage, UniWebViewNativeResultPayload payload)
        {
            Debug.LogError($"[WebViewService] Error received: {errorCode}; {errorMessage}]");
        }

        private bool OnShouldClose(UniWebView _) => false;

        private void TryOpenExternalApp(string url)
        {
#if UNITY_ANDROID
            (IsInstalled(url) ? (Action)(() => OpenAndroid(url)) : () => Debug.LogWarning($"App for '{url}' missing"))();
#else
            Application.OpenURL(url);
#endif
        }

#if UNITY_ANDROID
        private static bool IsInstalled(string url)
        {
            try
            {
                using var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var act = up.GetStatic<AndroidJavaObject>("currentActivity");
                var pm = act.Call<AndroidJavaObject>("getPackageManager");
                var it = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW");
                var uri = new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("parse", url);
                it.Call<AndroidJavaObject>("setData", uri);
                return pm.Call<AndroidJavaObject>("resolveActivity", it, 0) != null;
            }
            catch { return false; }
        }

        private static void OpenAndroid(string url)
        {
            try
            {
                using var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var act = up.GetStatic<AndroidJavaObject>("currentActivity");
                var it = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW");
                var uri = new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("parse", url);
                it.Call<AndroidJavaObject>("setData", uri);
                act.Call("startActivity", it);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"OpenURL error: {e.Message}");
            }
        }
#endif

        private static float GetKeyboardHeight()
        {
#if UNITY_ANDROID
            try
            {
                using var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var act = up.GetStatic<AndroidJavaObject>("currentActivity");
                var view = act.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
                var r = new AndroidJavaObject("android.graphics.Rect");
                view.Call("getWindowVisibleDisplayFrame", r);
                return Screen.height - r.Call<int>("height");
            }
            catch { return 0; }
#elif UNITY_IOS
            return TouchScreenKeyboard.area.height;
#else
            return 0;
#endif
        }

        private float GetGestureBarHeight()
        {
            if (_gestureBar >= 0) return _gestureBar;
#if UNITY_ANDROID
            try
            {
                using var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var act = up.GetStatic<AndroidJavaObject>("currentActivity");

                var metrics = new AndroidJavaObject("android.util.DisplayMetrics");
                act.Call<AndroidJavaObject>("getWindowManager")
                   .Call<AndroidJavaObject>("getDefaultDisplay")
                   .Call("getRealMetrics", metrics);

                int realH = metrics.Get<int>("heightPixels");
                var view = act.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
                var r = new AndroidJavaObject("android.graphics.Rect");
                view.Call("getWindowVisibleDisplayFrame", r);

                _gestureBar = realH - r.Call<int>("height");
            }
            catch { _gestureBar = 0; }
#elif UNITY_IOS
            _gestureBar = TouchScreenKeyboard.area.height;
#else
            _gestureBar = 0;
#endif
            return _gestureBar;
        }

        private void Cleanup()
        {
            if (_webView is not null)
            {
                _webView.OnShouldClose -= OnShouldClose;
                _webView.OnMessageReceived -= OnMessage;
                _webView = null;
            }

            if (_viewGO)
                Destroy(_viewGO);
            if (_bgGO)
                Destroy(_bgGO);

            _viewGO = _bgGO = null;
        }
    }
}
#endif