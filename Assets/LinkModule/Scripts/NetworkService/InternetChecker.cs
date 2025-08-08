using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinkModule.Scripts.Helper;
using UnityEngine;
using UnityEngine.Networking;

namespace LinkModule.Scripts.NetworkService
{
    public static class InternetChecker
    {
        private const int TIMEOUT_SECONDS = 5;
        private const int FAST_TIMEOUT_SECONDS = 2;
        private const int MAX_CONCURRENT_REQUESTS = 2;
        private const int REQUEST_DELAY_MILLISECONDS = 3000; // Delay between requests to avoid overwhelming servers
        
        private static readonly string[] PrimaryUrls = {
            "https://1.1.1.1",     // Cloudflare DNS
            "https://8.8.8.8",     // Google DNS
            "https://208.67.222.222" // OpenDNS
        };
        
        private static readonly string[] FallbackUrls = {
            "https://www.google.com/generate_204",
            "https://www.cloudflare.com",
            "https://httpbin.org/status/200"
        };

        /// <summary>
        /// Asynchronously checks for internet connectivity with configurable timeout.
        /// Safe to call from non-async methods (fire-and-forget).
        /// </summary>
        /// <param name="onResult">Callback invoked on the main thread with a connectivity result</param>
        /// <param name="timeoutSeconds">Overall timeout for the check operation</param>
        public static void CheckInternetAsync(Action<bool> onResult, int timeoutSeconds = 10)
        {
            _ = CheckAndHandleAsync(onResult, timeoutSeconds);
        }

        /// <summary>
        /// Asynchronously checks for internet connectivity and returns the result.
        /// </summary>
        /// <param name="timeoutSeconds">Overall timeout for the check operation</param>
        /// <returns>True if internet is available, false otherwise</returns>
        public static async Task<bool> CheckInternetAsync(int timeoutSeconds = 10)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                return await InternalCheckAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[InternetChecker] Check timed out");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InternetChecker] Exception during internet check: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Internal async method with exception handling and callback invocation.
        /// </summary>
        private static async Task CheckAndHandleAsync(Action<bool> onResult, int timeoutSeconds)
        {
            bool isConnected = await CheckInternetAsync(timeoutSeconds);
            
            try
            {
                // Ensure callback is invoked on the main thread
                if (MainThreadDispatcher.IsMainThread())
                {
                    onResult?.Invoke(isConnected);
                }
                else
                {
                    MainThreadDispatcher.Enqueue(() => onResult?.Invoke(isConnected));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InternetChecker] Exception in callback: {ex}");
            }
        }

        /// <summary>
        /// Performs connectivity check using a tiered approach: fast primary check, then comprehensive fallback.
        /// </summary>
        private static async Task<bool> InternalCheckAsync(CancellationToken cancellationToken = default)
        {
            // First, try a fast check with primary URLs
            if (await QuickConnectivityCheck(cancellationToken))
                return true;

            // If quick check fails, try a more comprehensive check
            return await ComprehensiveConnectivityCheck(cancellationToken);
        }

        /// <summary>
        /// Performs a quick connectivity check using DNS servers with short timeout.
        /// </summary>
        private static async Task<bool> QuickConnectivityCheck(CancellationToken cancellationToken)
        {
            var tasks = new List<Task<bool>>();
            
            // Start requests with staggered delays to avoid overwhelming servers
            for (int i = 0; i < Math.Min(MAX_CONCURRENT_REQUESTS, PrimaryUrls.Length); i++)
            {
                if (i > 0)
                {
                    await Task.Delay(REQUEST_DELAY_MILLISECONDS, cancellationToken);
                }
                
                var url = PrimaryUrls[i];
                tasks.Add(CheckSingleUrlAsync(url, FAST_TIMEOUT_SECONDS, cancellationToken));
            }

            try
            {
                // Return true if any request succeeds quickly
                var completedTask = await Task.WhenAny(tasks);
                return await completedTask;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[InternetChecker] Quick check failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Performs a comprehensive connectivity check using multiple URL types.
        /// </summary>
        private static async Task<bool> ComprehensiveConnectivityCheck(CancellationToken cancellationToken)
        {
            var allUrls = PrimaryUrls.Concat(FallbackUrls).ToArray();
            var semaphore = new SemaphoreSlim(MAX_CONCURRENT_REQUESTS, MAX_CONCURRENT_REQUESTS);
            var delayCounter = 0;
            
            var tasks = allUrls.Select(async url =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    // Add staggered delay to avoid overwhelming servers
                    var currentDelay = Interlocked.Increment(ref delayCounter);
                    if (currentDelay > 1)
                    {
                        await Task.Delay(REQUEST_DELAY_MILLISECONDS * (currentDelay - 1), cancellationToken);
                    }
                    
                    return await CheckSingleUrlAsync(url, TIMEOUT_SECONDS, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();

            try
            {
                // Wait for the first successful result
                while (tasks.Count > 0)
                {
                    var completedTask = await Task.WhenAny(tasks);
                    tasks.Remove(completedTask);
                    
                    if (await completedTask)
                        return true;
                }
                
                return false;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InternetChecker] Comprehensive check failed: {ex}");
                return false;
            }
            finally
            {
                semaphore?.Dispose();
            }
        }
        
        /// <summary>
        /// Checks connectivity to a single URL with specified timeout.
        /// </summary>
        private static async Task<bool> CheckSingleUrlAsync(string url, int timeoutSeconds, CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = UnityWebRequest.Head(url);
                request.timeout = timeoutSeconds;
                
                var operation = request.SendWebRequest();
                
                // Wait for completion or cancellation
                while (!operation.isDone && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Yield();
                }
                
                if (cancellationToken.IsCancellationRequested)
                {
                    request.Abort();
                    return false;
                }

                bool isSuccess = request.result == UnityWebRequest.Result.Success;
                
                if (!isSuccess)
                {
                    Debug.Log($"[InternetChecker] Failed to reach {url}: {request.error}");
                }
                
                return isSuccess;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[InternetChecker] Exception checking {url}: {ex.Message}");
                return false;
            }
        }
    }
}