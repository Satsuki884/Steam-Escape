#if USE_FIREBASE_SDK
using System;
using Firebase;
using Firebase.Extensions;
using Firebase.Messaging;
using UnityEngine;
#if UNITY_ANDROID
using System.Collections;
using UnityEngine.Android;
#endif

namespace LinkModule.Scripts.MessageService
{
    public class MessageService : MonoBehaviour, IMessageService
    {
        public event Action<string> OnPushTokenReceived;
        public event Action<string> OnPushTokenFailed;
        public event Action<FirebaseMessage> OnMessageReceived;

        private string _pushToken = "";
        private bool _waitingForNotificationPermission = false;
        private Coroutine _permissionCoroutine;

#if UNITY_ANDROID
        private const string POST_NOTIFICATIONS_PERMISSION = "android.permission.POST_NOTIFICATIONS";
#endif

        public void Initialize()
        {
#if UNITY_ANDROID
            if (CheckAndroidAPI(33))
            {
                if (!Permission.HasUserAuthorizedPermission(POST_NOTIFICATIONS_PERMISSION))
                {
                    _waitingForNotificationPermission = true;
                    Permission.RequestUserPermission(POST_NOTIFICATIONS_PERMISSION);
                    _permissionCoroutine = StartCoroutine(WaitForNotificationPermissionCoroutine());
                    return;
                }
            }
#endif
            StartFirebaseMessaging(); 
        }
        
        private static bool CheckAndroidAPI(int apiLevel)
        {
#if UNITY_ANDROID
            try
            {
                using var version = new AndroidJavaClass("android.os.Build$VERSION");
                int sdkInt = version.GetStatic<int>("SDK_INT");
                return sdkInt >= apiLevel;
            }
            catch
            {
                return false;
            }
#else
    return false;
#endif
        }

        private void StartFirebaseMessaging()
        {
            Debug.Log("[MessageService] Starting Firebase Messaging init...");
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                
                if (dependencyStatus == DependencyStatus.Available)
                {
                    FirebaseMessaging.TokenReceived += OnTokenReceivedHandler;

                    // Explicit token request (will also cover "lost" tokens)
                    FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(tokenTask =>
                    {
                        if (tokenTask.IsCompleted && !tokenTask.IsFaulted && !tokenTask.IsCanceled)
                        {
                            var token = tokenTask.Result;
                            PushTokenReceived(token);
                        }
                        else
                        {
                            OnPushTokenFailed?.Invoke(tokenTask.Exception?.Message);
                        }
                    });
                }
                else
                {
                    OnPushTokenFailed?.Invoke(dependencyStatus.ToString());
                }
            });
        }

#if UNITY_ANDROID
        private IEnumerator WaitForNotificationPermissionCoroutine(float timeout = 90)
        {
            float elapsed = 0f;
            while (elapsed < timeout)
            {
                yield return new WaitForSecondsRealtime(0.1f);
                if (Permission.HasUserAuthorizedPermission(POST_NOTIFICATIONS_PERMISSION))
                {
                    StartFirebaseMessaging();
                    yield break;
                }
                elapsed += 0.1f;
            }
            OnPushTokenFailed?.Invoke("[MessageService] Notification permission NOT granted. Starting FCM anyway (pushes will not work)");
            StartFirebaseMessaging();
        }
        
        private void OnApplicationPause(bool paused)
        {
            if (!paused && _waitingForNotificationPermission)
            {
                Debug.Log("[MessageService] App returned from background during permission request. Rechecking notification permission.");
                _waitingForNotificationPermission = false;
                if (_permissionCoroutine != null)
                {
                    StopCoroutine(_permissionCoroutine);
                    _permissionCoroutine = null;
                }
                StartFirebaseMessaging();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && _waitingForNotificationPermission)
            {
                Debug.Log("[MessageService] App gained focus during permission request. Rechecking notification permission.");
                _waitingForNotificationPermission = false;
                if (_permissionCoroutine != null)
                {
                    StopCoroutine(_permissionCoroutine);
                    _permissionCoroutine = null;
                }
                StartFirebaseMessaging();
            }
        }
#endif

        private void OnTokenReceivedHandler(object sender, TokenReceivedEventArgs token)
        {
            PushTokenReceived(token.Token);
        }

        private void PushTokenReceived(string token)
        {
            if (_pushToken == token)
                return; // Duplicate, skip event
            
            _pushToken = token;
            OnPushTokenReceived?.Invoke(_pushToken);
        }

        private void OnDestroy()
        {
            if (_permissionCoroutine != null)
                StopCoroutine(_permissionCoroutine);
            
            FirebaseMessaging.TokenReceived -= OnTokenReceivedHandler;
        }
    }
}
#endif