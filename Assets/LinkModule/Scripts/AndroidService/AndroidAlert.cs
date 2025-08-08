using System;
using LinkModule.Scripts.Helper;
using UnityEngine;

namespace LinkModule.Scripts.AndroidService
{
    public static class AndroidAlert
    {
#if UNITY_ANDROID //&& !UNITY_EDITOR
        /// <summary>
        /// Show AlertDialog with customizable buttons.
        /// </summary>
        /// <param name="title">Dialog title.</param>
        /// <param name="message">Dialog message</param>
        /// <param name="onOk">Callback for positive button.</param>
        /// <param name="onCancel">Callback for negative button (optional).</param>
        /// <param name="onDismiss">Callback for dialog dismissal (optional).</param>
        /// <param name="positiveButtonText">Text for positive button (default: "OK").</param>
        /// <param name="negativeButtonText">Text for negative button (default: "Cancel").</param>
        /// <param name="cancelable">Whether dialog can be canceled by back button or outside touch (default: true).</param>
        public static void ShowAlert(
            string title, 
            string message, 
            Action onOk = null, 
            Action onCancel = null,
            Action onDismiss = null,
            string positiveButtonText = "OK",
            string negativeButtonText = "Cancel",
            bool cancelable = true)
        {
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(message))
            {
                Debug.LogWarning("AndroidAlert: Both title and message are empty");
                onOk?.Invoke();
                return;
            }

            // Ensure we're on the main thread
            if (Application.platform == RuntimePlatform.Android)
            {
                try
                {
                    using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                        
                    if (activity == null)
                    {
                        Debug.LogError("AndroidAlert: Current activity is null");
                        onOk?.Invoke();
                        return;
                    }

                    // Run on UI thread to ensure proper dialog display
                    activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        ShowAlertInternal(activity, title, message, onOk, onCancel, onDismiss, 
                            positiveButtonText, negativeButtonText, cancelable);
                    }));
                }
                catch (Exception e)
                {
                    Debug.LogError($"AndroidAlert: Exception in ShowAlert: {e}");
                    onOk?.Invoke();
                }
            }
        }

        private static void ShowAlertInternal(
            AndroidJavaObject activity,
            string title,
            string message,
            Action onOk,
            Action onCancel,
            Action onDismiss,
            string positiveButtonText,
            string negativeButtonText,
            bool cancelable)
        {
            try
            {
                using var alertBuilder = new AndroidJavaObject("android.app.AlertDialog$Builder", activity);
                // Set title and message
                if (!string.IsNullOrEmpty(title))
                    alertBuilder.Call<AndroidJavaObject>("setTitle", title);
                    
                if (!string.IsNullOrEmpty(message))
                    alertBuilder.Call<AndroidJavaObject>("setMessage", message);

                // Set cancelable
                alertBuilder.Call<AndroidJavaObject>("setCancelable", cancelable);

                // Positive button (OK)
                if (!string.IsNullOrEmpty(positiveButtonText))
                {
                    alertBuilder.Call<AndroidJavaObject>("setPositiveButton", positiveButtonText, 
                        new OnClickListener(onOk));
                }

                // Negative button (Cancel) - only add if callback provided or explicit text given
                if (onCancel != null || negativeButtonText != "Cancel")
                {
                    alertBuilder.Call<AndroidJavaObject>("setNegativeButton", negativeButtonText, 
                        new OnClickListener(onCancel));
                }

                // Create and configure dialog
                using var dialog = alertBuilder.Call<AndroidJavaObject>("create");
                // Handle dialog dismissal (back button, outside touch)
                if (onDismiss != null)
                {
                    dialog.Call("setOnDismissListener", new OnDismissListener(onDismiss));
                }

                // Show dialog
                dialog.Call("show");
            }
            catch (Exception e)
            {
                Debug.LogError($"AndroidAlert: Exception in ShowAlertInternal: {e}");
                onOk?.Invoke();
            }
        }

        /// <summary>
        /// Show a simple alert with only an OK button.
        /// </summary>
        public static void ShowSimpleAlert(string title, string message, Action onOk = null)
        {
            ShowAlert(title, message, onOk, null, null, "OK", null, true);
        }

        /// <summary>
        /// Show a confirmation dialog with OK and Cancel buttons.
        /// </summary>
        public static void ShowConfirmationDialog(string title, string message, Action onConfirm, Action onCancel = null)
        {
            ShowAlert(title, message, onConfirm, onCancel, null, "OK", "Cancel", true);
        }

        /// <summary>
        /// Show Android Toast message.
        /// </summary>
        /// <param name="message">Toast message</param>
        /// <param name="longDuration">Use long duration instead of short</param>
        public static void ShowAndroidToast(string message, bool longDuration = false)
        {
            if (string.IsNullOrEmpty(message))
            {
                Debug.LogWarning("AndroidAlert: Toast message is empty");
                return;
            }

            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                if (activity == null)
                {
                    Debug.LogWarning("AndroidAlert: Activity is null, cannot show toast");
                    return;
                }

                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    ShowToastInternal(activity, message, longDuration);
                }));
            }
            catch (Exception e)
            {
                Debug.LogError($"AndroidAlert: Toast error: {e}");
            }
        }

        private static void ShowToastInternal(AndroidJavaObject activity, string message, bool longDuration)
        {
            try
            {
                using var toastClass = new AndroidJavaClass("android.widget.Toast");
                int duration = longDuration ? 
                    toastClass.GetStatic<int>("LENGTH_LONG") : 
                    toastClass.GetStatic<int>("LENGTH_SHORT");

                using var toast = toastClass.CallStatic<AndroidJavaObject>("makeText", activity, message, duration);
                toast?.Call("show");
            }
            catch (Exception e)
            {
                Debug.LogError($"AndroidAlert: Toast internal error: {e}");
            }
        }

        /// <summary>
        /// A class that implements DialogInterface.OnClickListener
        /// </summary>
        private class OnClickListener : AndroidJavaProxy
        {
            private readonly Action _callback;

            public OnClickListener(Action callback) : base("android.content.DialogInterface$OnClickListener")
            {
                _callback = callback;
            }

            public void onClick(AndroidJavaObject dialog, int which)
            {
                try
                {
                    if (_callback != null)
                        MainThreadDispatcher.Enqueue(_callback);
                }
                catch (Exception e)
                {
                    Debug.LogError($"AndroidAlert: OnClickListener callback error: {e}");
                }
            }
        }

        /// <summary>
        /// A class that implements DialogInterface.OnDismissListener
        /// </summary>
        private class OnDismissListener : AndroidJavaProxy
        {
            private readonly Action _callback;

            public OnDismissListener(Action callback) : base("android.content.DialogInterface$OnDismissListener")
            {
                _callback = callback;
            }

            public void onDismiss(AndroidJavaObject dialog)
            {
                try
                {
                    if (_callback != null)
                        MainThreadDispatcher.Enqueue(_callback);
                }
                catch (Exception e)
                {
                    Debug.LogError($"AndroidAlert: OnDismissListener callback error: {e}");
                }
            }
        }

        /// <summary>
        /// A class that implements Runnable for UI thread operations
        /// </summary>
        private class AndroidJavaRunnable : AndroidJavaProxy
        {
            private readonly Action _callback;

            public AndroidJavaRunnable(Action callback) : base("java.lang.Runnable")
            {
                _callback = callback;
            }

            public void run()
            {
                try
                {
                    if (_callback != null)
                        MainThreadDispatcher.Enqueue(_callback);
                }
                catch (Exception e)
                {
                    Debug.LogError($"AndroidAlert: Runnable callback error: {e}");
                }
            }
        }

#else
        // For non-Android platforms or editor, provide fallback implementations
        public static void ShowAlert(
            string title, 
            string message, 
            Action onOk = null, 
            Action onCancel = null,
            Action onDismiss = null,
            string positiveButtonText = "OK",
            string negativeButtonText = "Cancel",
            bool cancelable = true)
        {
            Debug.LogWarning($"AndroidAlert.ShowAlert: Not supported on this platform. Title: {title}, Message: {message}");
            onOk?.Invoke();
        }

        public static void ShowSimpleAlert(string title, string message, Action onOk = null)
        {
            Debug.LogWarning($"AndroidAlert.ShowSimpleAlert: Not supported on this platform. Title: {title}, Message: {message}");
            onOk?.Invoke();
        }

        public static void ShowConfirmationDialog(string title, string message, Action onConfirm, Action onCancel = null)
        {
            Debug.LogWarning($"AndroidAlert.ShowConfirmationDialog: Not supported on this platform. Title: {title}, Message: {message}");
            onConfirm?.Invoke();
        }

        public static void ShowAndroidToast(string message, bool longDuration = false)
        {
            Debug.LogWarning($"AndroidAlert.ShowAndroidToast: Not supported on this platform. Message: {message}");
        }
#endif
    }
}