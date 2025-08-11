using System.Collections;
using UnityEngine;
using LinkModule.Scripts.AdService;
using LinkModule.Scripts.AndroidService;
using LinkModule.Scripts.Config;
using LinkModule.Scripts.NetworkService;
using LinkModule.Scripts.Helper;
using TitanModulePackage.Scripts.SaveService;
// using LinkModule.Scripts.WebViewService;

#if USE_FACEBOOK_SDK || USE_APPSFLYER_SDK
using LinkModule.Scripts.DeepLinkService;
#endif
#if USE_FIREBASE_SDK
using LinkModule.Scripts.MessageService;
#endif
#if USE_INSTALL_REFERRER
using LinkModule.Scripts.ReferrerService;
#endif
#if USE_UNIWEBVIEW
using LinkModule.Scripts.WebViewService;
#endif

namespace LinkModule.Scripts.AppEntryPoint
{
    [DefaultExecutionOrder(1)]
    public class AppEntryPoint : MonoBehaviour
    {
        public const int SERVICE_STARTUP_TIMEOUT = 10;
        
        [SerializeField] private ServicesConfig config;
        [SerializeField] private LoadingScreenConfig loadingScreenConfig;
        private AppStartupManager _startupManager;
        private IServiceRegistry _serviceRegistry;
        private LoadingScreen _loadingScreen;
        private MainThreadDispatcher _mainThreadDispatcher;
        private AudioSource[] _audioSources;
        private Coroutine _timeoutCoroutine;

        private void Awake()
        {
            if (config is null)
            {
                Debug.LogError("[AppEntryPoint] ServicesConfig reference is missing!");
                enabled = false;
                return;
            }

            ServicesRegister();
            InjectConfigToServices(config);

            _startupManager = new AppStartupManager(
                config,
                loadingScreenConfig,
                _serviceRegistry
            );

            _mainThreadDispatcher = new GameObject("MainThreadDispatcher").AddComponent<MainThreadDispatcher>();
            _mainThreadDispatcher.transform.SetParent(transform);
            _loadingScreen = FindFirstObjectByType<LoadingScreen>();
            _startupManager.OnStateChanged += _loadingScreen.UpdateStateLoading;
            DontDestroyOnLoad(gameObject);
            SubscribeToManager();
        }

        private void Start()
        {
            _loadingScreen?.Initialize(loadingScreenConfig);
            InternetChecker.CheckInternetAsync(OnCheckInternetAvailable);

#if UNITY_6000_0_OR_NEWER
            _audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
#else
            _audioSources = FindObjectsOfType<AudioSource>();
#endif
            if (_audioSources is not null)
            {
                foreach (var audioSource in _audioSources)
                {
                    if (audioSource is null) 
                        continue;
                    audioSource.mute = true;
                    audioSource.Stop();
                }
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromManager();
            _serviceRegistry.Clear();
        }

        private void ServicesRegister()
        {
            _serviceRegistry = new ServiceRegistry();
            
            var httpService = NewService<HttpService>("HttpService");
            _serviceRegistry.Register<IHttpService>(httpService);
            
            var saveService = NewService<SaveService.SaveService>("SaveService");
            _serviceRegistry.Register<ISaveService>(saveService);

#if USE_UNIWEBVIEW
            var webViewService = NewService<WebViewService.WebViewService>("WebViewService");
            _serviceRegistry.Register<IWebViewService>(webViewService);
#endif
#if USE_INSTALL_REFERRER
            var installReferrerService = NewService<InstallReferrerService>("InstallReferrerService");
            _serviceRegistry.Register<IInstallReferrerService>(installReferrerService);
#endif
#if USE_FIREBASE_SDK
            var messageService = NewService<MessageService.MessageService>("MessageService");
            _serviceRegistry.Register<IMessageService>(messageService);
#endif
#if USE_APPSFLYER_SDK
            var appsFlyerService = NewService<AppsFlyerService>("AppsFlyerService");
            _serviceRegistry.Register<IAppsFlyerService>(appsFlyerService);
#endif
#if USE_FACEBOOK_SDK
            var deepLinkService = NewService<DeepLinkService.DeepLinkService>("DeepLinkService");
            _serviceRegistry.Register<IDeepLinkService>(deepLinkService);
#endif
#if USE_FACEBOOK_SDK || USE_APPSFLYER_SDK 
            var adService = NewService<LinkModule.Scripts.AdService.AdService>("AdService");
            _serviceRegistry.Register<IAdService>(adService);
#endif
        }
        
        /// <summary>Checks internet and triggers initial services if available.</summary>
        private void OnCheckInternetAvailable(bool isInternetAvailable)
        {
            if (!isInternetAvailable)
                ShowConnectionLostAlert();
            else
            {
                _loadingScreen.Show();
                if (!_serviceRegistry.Get<ISaveService>().LoadIsOpenFirstTime())
                {
                    FinalizeStartup();
                    _loadingScreen.ForceFinish();
                }
                else
                {
                    _startupManager.InitializeServices();
                    _timeoutCoroutine = StartCoroutine(TimeoutCoroutine());
                }
            }
        }

        private IEnumerator TimeoutCoroutine()
        {
            var startTime = Time.time;
            yield return new WaitForSeconds(SERVICE_STARTUP_TIMEOUT);
            var endTime = Time.time;
            Debug.LogWarning("[AppEntryPoint] Services timed out!");
            if (config.isTest)
                _startupManager.ServicesTimeout(endTime - startTime);
            FinalizeStartup();
        }

        private void FinalizeStartup()
        {
            if (_timeoutCoroutine is not null)
            {
                StopCoroutine(_timeoutCoroutine);
                _timeoutCoroutine = null;
            }
            
            if (_serviceRegistry.Get<IWebViewService>().IsWebViewOpened())
                return;

            _loadingScreen?.Hide();
            _startupManager.BuildTrackingUrl();
            
            if (!_startupManager.IsOpenFirstTime)
            {
                if (config.isTest)
                {
                    if (config.runGame) 
                        LaunchGame();
                    else 
                        _startupManager.ShowDiagnostics();
                }
                else if (_startupManager.OpenGame)
                {
                    LaunchGame();
                }
                else
                {
                    OpenWebView(_startupManager.FinalUrl);
                }
                return;
            }
            
            _serviceRegistry.Get<ISaveService>().SaveUrl(_startupManager.FinalUrl);
            _serviceRegistry.Get<ISaveService>().SaveIsOpenFirstTime(false);
            if (config.isTest)
            {
                if (config.runGame) 
                    LaunchGame();
                else 
                    _startupManager.ShowDiagnostics();
            }
            else
            {
                OpenWebView(_startupManager.FinalUrl);
            }
        }

        private void ShowConnectionLostAlert()
        {
            AndroidAlert.ShowAlert("Connection Lost", "Looks like you're offline. Please reconnect.",
                () => InternetChecker.CheckInternetAsync(OnCheckInternetAvailable),
                () => InternetChecker.CheckInternetAsync(OnCheckInternetAvailable));
        }

        private void OpenWebView(string url)
        {
            _serviceRegistry.Get<IWebViewService>()?.LoadUrl(url);
        }

        private void OnLaunchGameRequested()
        {
            _serviceRegistry.Get<ISaveService>().SaveOpenGame(true);
            LaunchGame();
        }

        /// <summary>
        /// Handles scene loading, audio, and final transition into the game.
        /// </summary>
        private void LaunchGame()
        {
            _loadingScreen?.Hide();

            if (_audioSources != null)
            {
                foreach (var audioSource in _audioSources)
                {
                    if (audioSource is null) 
                        continue;
                    audioSource.mute = false;
                    audioSource.Play();
                }
            }

            int nextIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextIndex);

            Screen.autorotateToLandscapeLeft = config.autorotateToLandscapeLeft;
            Screen.autorotateToLandscapeRight = config.autorotateToLandscapeRight;
            Screen.autorotateToPortrait = config.autorotateToPortrait;
            Screen.autorotateToPortraitUpsideDown = config.autorotateToPortraitUpsideDown;
            Screen.orientation = config.orientationInGame;

            if (_loadingScreen is not null)
            {
                _startupManager.OnStateChanged -= _loadingScreen.UpdateStateLoading;
                Destroy(_loadingScreen.gameObject);
            }
            
            Destroy(gameObject);
        }

        private T NewService<T>(string goName) where T : MonoBehaviour
        {
            var go = new GameObject(goName);
            go.transform.SetParent(transform, false);
            return go.AddComponent<T>();
        }

        private void InjectConfigToServices(ServicesConfig configData)
        {
            foreach (var svc in GetComponentsInChildren<MonoBehaviour>())
            {
                if (svc is IConfigurable<ServicesConfig> configurable)
                    configurable.SetConfig(configData);
            }
        }

        private void SubscribeToManager()
        {
            _startupManager.AllServicesInitialized += FinalizeStartup;
            _startupManager.LaunchGameRequested += OnLaunchGameRequested;
            _startupManager.SubscribeToServices();
        }

        private void UnsubscribeFromManager()
        {
            _startupManager.AllServicesInitialized -= FinalizeStartup;
            _startupManager.LaunchGameRequested -= OnLaunchGameRequested;
            _startupManager.UnsubscribeFromServices();
        }
    }
}