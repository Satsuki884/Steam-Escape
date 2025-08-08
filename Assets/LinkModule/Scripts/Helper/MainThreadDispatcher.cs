using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LinkModule.Scripts.Helper
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RunOnMainThreadAttribute : Attribute
    {
    }
    
    public static class MainThreadInvoker
    {
        public static void Invoke(object target, string methodName, params object[] parameters)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
                throw new MissingMethodException(target.GetType().Name, methodName);

            if (method.GetCustomAttribute<RunOnMainThreadAttribute>() == null)
                throw new InvalidOperationException($"Method {methodName} is not marked with [RunOnMainThread].");

            MainThreadDispatcher.Enqueue(() => method.Invoke(target, parameters));
        }

        public static async Task<T> InvokeAsync<T>(object target, string methodName, params object[] parameters)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
                throw new MissingMethodException(target.GetType().Name, methodName);

            if (method.GetCustomAttribute<RunOnMainThreadAttribute>() == null)
                throw new InvalidOperationException($"Method {methodName} is not marked with [RunOnMainThread].");

            return await MainThreadDispatcher.Enqueue(() =>
            {
                var result = method.Invoke(target, parameters);
                return (T)result;
            });
        }
    }
    
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _instance;
        private static readonly ConcurrentQueue<Action> Actions = new();

        private static int _mainThreadId;

        public static bool IsInitialized => _instance is not null;

        private void Awake()
        {
            if (_instance is null)
            {
                _instance = this;
                _mainThreadId = Thread.CurrentThread.ManagedThreadId;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            while (Actions.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MainThreadDispatcher] Exception in queued action: {ex}");
                }
            }
        }

        public static void Enqueue(Action action)
        {
            if (action == null) 
                throw new ArgumentNullException(nameof(action));

            if (IsMainThread())
            {
                action.Invoke();
            }
            else
            {
                Actions.Enqueue(action);
            }
        }

        public static Task<T> Enqueue<T>(Func<T> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var tcs = new TaskCompletionSource<T>();

            Enqueue(() =>
            {
                try
                {
                    var result = func();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        public static Task Enqueue(Func<Task> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var tcs = new TaskCompletionSource<bool>();

            Enqueue(async () =>
            {
                try
                {
                    await func();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        public static Task<T> Enqueue<T>(Func<Task<T>> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var tcs = new TaskCompletionSource<T>();

            Enqueue(async () =>
            {
                try
                {
                    var result = await func();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        public static bool IsMainThread()
        {
            return Thread.CurrentThread.ManagedThreadId == _mainThreadId;
        }

        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // private static void Initialize()
        // {
        //     if (_instance == null)
        //     {
        //         var dispatcherObject = new GameObject("MainThreadDispatcher");
        //         _instance = dispatcherObject.AddComponent<MainThreadDispatcher>();
        //     }
        // }
    }
}