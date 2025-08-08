using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using LinkModule.Scripts.AndroidService;
using LinkModule.Scripts.Config;
using LinkModule.Scripts.Helper;
using LinkModule.Scripts.NetworkService;
using LinkModule.Scripts.WebViewService;
using TitanModulePackage.Scripts.SaveService;

#if USE_FACEBOOK_SDK
using Facebook.Unity;
#endif
#if USE_FACEBOOK_SDK || USE_APPSFLYER_SDK || USE_INSTALL_REFERRER
using LinkModule.Scripts.DeepLinkService;
using LinkModule.Scripts.AdService;
#endif
#if USE_FIREBASE_SDK
using LinkModule.Scripts.MessageService;
#endif
#if USE_INSTALL_REFERRER
using LinkModule.Scripts.ReferrerService;
#endif

namespace LinkModule.Scripts.AppEntryPoint
{
    [Flags]
    public enum InitFlag
    {
        None = 0,
#if USE_INSTALL_REFERRER
        InstallReferrer = 1 << 0,
#endif
#if USE_FIREBASE_SDK
        Push = 1 << 1,
#endif
#if USE_FACEBOOK_SDK || USE_APPSFLYER_SDK
        DeepLink = 1 << 2,
        AdId = 1 << 3
#endif
    }
    
    public enum StartupState
    {
        NotStarted,
        Initializing,
        ServicesReady,
        PostInitializing,
        LoadFromSave,
        Failed,
        TimedOut,
#if USE_INSTALL_REFERRER
        InstallReferrerInitialize,
        InstallReferrerSuccess,
        InstallReferrerFailure,
#endif
#if USE_FIREBASE_SDK
        FirebaseInitialize,
        FirebaseSuccess,
        FirebaseFailure,
#endif
#if USE_FACEBOOK_SDK
        FBInitialize,
        FBDeepLinkSuccess,
        FBDeepLinkFailure,
#endif
#if USE_APPSFLYER_SDK
        AppsFlyerInitialize,
        AppsFlyerCampaignSuccess,
        AppsFlyerCampaignFailure,
#endif
        OpenWebView
    }

    /// <summary>
    /// Handles service initialization, event orchestration, and URL building.
    /// </summary>
    public class AppStartupManager
    {
        public event Action<StartupState> OnStateChanged;
        public event Action AllServicesInitialized;
        public event Action LaunchGameRequested;
        public event Action<string> ShowDiagnosticsRequested;
        
        public StartupState CurrentState { get; private set; } = StartupState.NotStarted;
        
#if USE_INSTALL_REFERRER
        public string ReferrerRawData { get; private set; }
        public string UtmContent { get; private set; }
        public string ReferrerResponse { get; private set; }
        public ReferrerData ReferrerData { get; private set; }
#endif
#if USE_FIREBASE_SDK
        public string PushToken { get; private set; }
        public string PushTokenFailed { get; private set; }
#endif
#if USE_FACEBOOK_SDK
        public string DeepLinkError { get; private set; }
        public string FbDataResponse { get; private set; }
        public FbData FbData { get; private set; }
#endif
#if USE_APPSFLYER_SDK
        public string CampaignRawData { get; private set; }
        public string DeviceId { get; private set; }
#endif
#if USE_FACEBOOK_SDK || USE_APPSFLYER_SDK
        public string DeepLink { get; private set; }
        public string Campaign { get; private set; }
        public string AdId { get; private set; }
#endif
        public string FinalUrl { get; private set; }
        public bool IsOpenFirstTime { get; private set; }
        public bool OpenGame { get; private set; }
        
        private readonly ServicesConfig _config;
        private LoadingScreenConfig _loadingScreenConfig;
        private readonly IServiceRegistry _services;
        private InitFlag _completedFlags;
        private readonly InitFlag _expectedFlags;
        private string _completedState;
        private bool _servicesTimeout = false;
        private float _timeTimeout;
        private bool _isPostInitialized = false;

        public AppStartupManager(
            ServicesConfig config,
            LoadingScreenConfig loadingScreenConfig,
            IServiceRegistry services
        )
        {
            _config = config;
            _loadingScreenConfig = loadingScreenConfig;
            _services = services;
            _expectedFlags = GetExpectedFlags();
        }

        /// <summary>Initializes essential services.</summary>
        public void InitializeServices()
        {
            ChangeState(StartupState.Initializing);

            IsOpenFirstTime = _services.Get<ISaveService>().LoadIsOpenFirstTime();
            
            if (!IsOpenFirstTime)
            {
                ChangeState(StartupState.LoadFromSave);
                OpenGame = _services.Get<ISaveService>().LoadOpenGame();
                return;
            }
            
#if USE_FACEBOOK_SDK
            if (string.IsNullOrEmpty(_config.urlIdToken))
            {
                _services.Get<IDeepLinkService>()?.Initialize();
                ChangeState(StartupState.FBInitialize);
            }
            else
            {
                _services.Get<IHttpService>().Get(_config.urlIdToken, response =>
                {
                    FbDataResponse = response;
                    FbData = ServerDataParser.ParseSimple(response);
                    _services.Get<IDeepLinkService>()?.SetFacebookCredentials(FbData.fid, FbData.ftok);
                    _services.Get<IDeepLinkService>()?.Initialize();
                    ChangeState(StartupState.FBInitialize);
                }, error =>
                {
                    DeepLinkError =  error;
                    _services.Get<IDeepLinkService>()?.Initialize();
                    ChangeState(StartupState.FBInitialize);
                });
            }
#endif
#if USE_APPSFLYER_SDK && !USE_FACEBOOK_SDK
            _services.Get<IAppsFlyerService>()?.Initialize();
            ChangeState(StartupState.AppsFlyerInitialize);
#endif
#if USE_INSTALL_REFERRER
            _services.Get<IInstallReferrerService>()?.Initialize();
            ChangeState(StartupState.InstallReferrerInitialize);
#endif
        }

        /// <summary>Initializes post-dependent services.</summary>
        public void PostInitializeServices()
        {
            if (_isPostInitialized)
                return;
            
#if USE_FIREBASE_SDK
            _services.Get<IMessageService>()?.Initialize();
#endif
#if USE_FACEBOOK_SDK || USE_APPSFLYER_SDK || USE_INSTALL_REFERRER
            _services.Get<IAdService>()?.Initialize();
#endif
            
            
            _isPostInitialized = true;
            ChangeState(StartupState.PostInitializing);
        }

        public void SubscribeToServices() => SubscribeOrUnsubscribeServices(true);
        public void UnsubscribeFromServices() => SubscribeOrUnsubscribeServices(false);

        /// <summary>
        /// Subscribe or unsubscribe all services using a single routine (avoids duplicate code).
        /// </summary>
        private void SubscribeOrUnsubscribeServices(bool subscribe)
        {
#if USE_UNIWEBVIEW
            if (subscribe)
            {
                _services.Get<IWebViewService>().OnWebViewClose += OnWebViewClose;
            }
            else
            {
                _services.Get<IWebViewService>().OnWebViewClose -= OnWebViewClose;
            }
#endif
            
#if USE_INSTALL_REFERRER
            if (subscribe)
            {
                _services.Get<IInstallReferrerService>().OnRawReferrerReceived += OnInstallReferrerRaw;
                _services.Get<IInstallReferrerService>().OnReferrerParsed += OnInstallReferrer;
                _services.Get<IInstallReferrerService>().OnFailed += OnInstallReferrerFailed;
            }
            else
            {
                _services.Get<IInstallReferrerService>().OnRawReferrerReceived -= OnInstallReferrerRaw;
                _services.Get<IInstallReferrerService>().OnReferrerParsed -= OnInstallReferrer;
                _services.Get<IInstallReferrerService>().OnFailed -= OnInstallReferrerFailed;
            }
#endif
            
#if USE_FACEBOOK_SDK
            if (subscribe)
            {
                _services.Get<IDeepLinkService>().OnDeepLinkReceived += OnDeepLinkReceived;
                _services.Get<IDeepLinkService>().OnDeeplinkFailed += OnDeepLinkFailed;
            }
            else
            {
                _services.Get<IDeepLinkService>().OnDeepLinkReceived -= OnDeepLinkReceived;
                _services.Get<IDeepLinkService>().OnDeeplinkFailed -= OnDeepLinkFailed;
            }
#endif
            
#if USE_APPSFLYER_SDK
            if (subscribe)
            {
                _services.Get<IAppsFlyerService>().OnCampaignReceived += OnCampaign;
                _services.Get<IAppsFlyerService>().OnCampaignFailed += OnCampaignFailed;
            }
            else
            {
                _services.Get<IAppsFlyerService>().OnCampaignReceived -= OnCampaign;
                _services.Get<IAppsFlyerService>().OnCampaignFailed -= OnCampaignFailed;
            }
#endif
            
#if USE_FIREBASE_SDK
            if (subscribe)
            {
                _services.Get<IMessageService>().OnPushTokenReceived += OnPushToken;
                _services.Get<IMessageService>().OnPushTokenFailed += OnPushTokenFailed;
            }
            else
            {
                _services.Get<IMessageService>().OnPushTokenReceived -= OnPushToken;
                _services.Get<IMessageService>().OnPushTokenFailed += OnPushTokenFailed;
            }
#endif
            
#if USE_FACEBOOK_SDK || USE_APPSFLYER_SDK || USE_INSTALL_REFERRER
            if (subscribe) 
                _services.Get<IAdService>().OnAdIdReceived += OnAdId;
            else 
                _services.Get<IAdService>().OnAdIdReceived -= OnAdId;
#endif
        }
        
#if USE_UNIWEBVIEW
        private void OnWebViewClose()
        {
            RequestLaunchGame();
        }
#endif
        
#if USE_INSTALL_REFERRER
        private void OnInstallReferrerRaw(string rawData) => ReferrerRawData = rawData;

        private void OnInstallReferrer(Dictionary<string, string> referrerData)
        {
            UtmContent = referrerData != null && referrerData.TryGetValue("utm_content", out var utm)
                ? utm : string.Empty;

            if (_config.decodeInstallReferrer && !string.IsNullOrEmpty(UtmContent))
            {
                _services.Get<IHttpService>().PostRaw(_config.installReferrerDecodeUrl, UtmContent,
                    response =>
                    {
                        ReferrerResponse = response;
                        ReferrerData = ServerDataParser.Parse(response);
                        ChangeState(StartupState.InstallReferrerSuccess);
                        SetReady(InitFlag.InstallReferrer);
                        PostInitializeServices();
                    },
                    error =>
                    {
                        ReferrerResponse = error;
                        ChangeState(StartupState.InstallReferrerFailure);
                        SetReady(InitFlag.InstallReferrer);
                    });
            }
            else if (!string.IsNullOrEmpty(UtmContent))
            {
                ChangeState(StartupState.InstallReferrerSuccess);
                SetReady(InitFlag.InstallReferrer);
                PostInitializeServices();
            }
            else
            {
                ChangeState(StartupState.InstallReferrerFailure);
                SetReady(InitFlag.InstallReferrer);
            }
        }

        private void OnInstallReferrerFailed(int errorCode, string errorMessage)
        {
            ReferrerRawData = $"Failed to get referrer data (error code: {errorCode}, message: {errorMessage}).";
            ChangeState(StartupState.InstallReferrerFailure);
            SetReady(InitFlag.InstallReferrer);
        }
#endif

#if USE_FIREBASE_SDK
        private void OnPushToken(string token)
        {
            PushToken = token;
            ChangeState(string.IsNullOrEmpty(PushToken) ? StartupState.FirebaseFailure : StartupState.FirebaseSuccess);
            SetReady(InitFlag.Push);
        }
        
        private void OnPushTokenFailed(string errorMessage)
        {
            PushTokenFailed = errorMessage;
            ChangeState(StartupState.FirebaseFailure);
            SetReady(InitFlag.Push);
        }
#endif

#if USE_FACEBOOK_SDK || USE_APPSFLYER_SDK
        private void OnAdId(string id)
        {
            AdId = id;
            SetReady(InitFlag.AdId);
        }
#endif

#if USE_FACEBOOK_SDK
        private void OnDeepLinkReceived(string link)
        {
            DeepLink = link;
            ChangeState(StartupState.FBDeepLinkSuccess);
            SetReady(InitFlag.DeepLink);
            PostInitializeServices();
        }

        private void OnDeepLinkFailed(string error)
        {
            DeepLinkError = error;
            ChangeState(StartupState.FBDeepLinkFailure);
#if USE_APPSFLYER_SDK
            _services.Get<IAppsFlyerService>()?.Initialize();
            ChangeState(StartupState.AppsFlyerInitialize);
#else
            SetReady(InitFlag.DeepLink);
#endif
        }
#endif

#if USE_APPSFLYER_SDK
        private void OnCampaign(string campaign)
        {
            Campaign = campaign;
            DeviceId = _services.Get<IAppsFlyerService>().DeviceId;
            ChangeState(StartupState.AppsFlyerCampaignSuccess);
            SetReady(InitFlag.DeepLink);
            PostInitializeServices();
        }

        private void OnCampaignFailed(string raw)
        {
            CampaignRawData = raw;
            ChangeState(StartupState.AppsFlyerCampaignFailure);
            SetReady(InitFlag.DeepLink);
        }
#endif

        /// <summary>Marks service as ready and finalizes if all flags are set.</summary>
        public void SetReady(InitFlag flag)
        {
            _completedFlags |= flag;
            if (_completedFlags != _expectedFlags) 
                return;
            
            ChangeState(StartupState.ServicesReady);
            AllServicesInitialized?.Invoke();
        }

        /// <summary>Constructs the final tracking URL using all state.</summary>
        public string BuildTrackingUrl()
        {
            var urlBuilder = new StringBuilder(_config.domain);
            bool hasQry = false;

            void Add(string key, string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    urlBuilder.Append(hasQry ? '&' : '?').Append(key).Append('=').Append(value);
                    hasQry = true;
                }
            }

#if USE_FACEBOOK_SDK || USE_APPSFLYER_SDK
            Add(_config.sub_id_1_Key, !string.IsNullOrEmpty(DeepLink) ? DeepLink : Campaign);
#endif
#if USE_FACEBOOK_SDK || USE_APPSFLYER_SDK
            Add(_config.ad_id_Key, AdId);
#endif
#if USE_FIREBASE_SDK
            Add(_config.push_token_Key, PushToken);
#endif
#if USE_APPSFLYER_SDK
            Add(_config.deviceID_Key, DeviceId);
#endif
#if USE_INSTALL_REFERRER
            if (ReferrerData != null && _config.decodeInstallReferrer)
            {
                Add(string.IsNullOrEmpty(DeepLink) ? _config.sub_id_1_Key : _config.campaign_group_name_ref_Key, ReferrerData.campaign_group_name);
                Add(_config.ad_id_ref_Key, ReferrerData.ad_id);
                Add(_config.ad_objective_name_ref_Key, ReferrerData.ad_objective_name);
                Add(_config.adgroup_id_ref_Key, ReferrerData.adgroup_id);
                Add(_config.adgroup_name_ref_Key, ReferrerData.adgroup_name);
                Add(_config.campaign_id_ref_Key, ReferrerData.campaign_id);
                Add(_config.campaign_name_ref_Key, ReferrerData.campaign_name);
                Add(_config.campaign_group_id_ref_Key, ReferrerData.campaign_group_id);
                Add(_config.account_id_ref_Key, ReferrerData.account_id);
                Add(_config.is_instagram_ref_Key, ReferrerData.is_instagram);
                Add(_config.is_an_ref_Key, ReferrerData.is_an);
                Add(_config.publisher_platform_ref_Key, ReferrerData.publisher_platform);
                Add(_config.platform_position_ref_Key, ReferrerData.platform_position);
            }
            else
            {
                Add(_config.naming_Key, UtmContent);
            }
#endif
            var url = _services.Get<ISaveService>().LoadUrl();
            FinalUrl = string.IsNullOrEmpty(url) ? urlBuilder.ToString() : url;
            return FinalUrl;
        }

        /// <summary>Requests diagnostics to be shown by the UI.</summary>
        public void ShowDiagnostics()
        {
            var report = new StringBuilder();
            report.AppendLine("==== [Startup Status] ====");
            report.AppendLine($"Completed State: {_completedState}");
            report.AppendLine($"Expected Flags: {_expectedFlags}");
            report.AppendLine($"Completed Flags: {_completedFlags}");
            report.AppendLine($"Open First Time: {IsOpenFirstTime.ToString()}");
            report.AppendLine($"Opened game: {OpenGame.ToString()}");
            report.AppendLine($"Services timeout: {_servicesTimeout.ToString()}");
            if (_servicesTimeout)
                report.AppendLine($"Time timeout: {_timeTimeout.ToString(CultureInfo.InvariantCulture)} sec");
            report.AppendLine();
            
            report.AppendLine("==== [Tracking Info] ====");
            report.AppendLine($"Domain: {_config.domain}");
            report.AppendLine($"Final URL: {FinalUrl}");
            report.AppendLine();

#if USE_INSTALL_REFERRER
            if (ReferrerData != null)
            {
                report.AppendLine("==== [Install Referrer] ====");
                report.AppendLine($"UTM Content: {UtmContent}");
                report.AppendLine($"Server Response: {ReferrerResponse}");
                report.AppendLine();
            }
            else
            {
                report.AppendLine("==== [Install Referrer] ====");
                report.AppendLine($"UTM Content: {UtmContent}");
                report.AppendLine();
            }
#endif
#if USE_APPSFLYER_SDK
            report.AppendLine("==== [AppsFlyer] ====");
            report.AppendLine($"Campaign: {Campaign}");
            report.AppendLine($"Campaign Error: {CampaignRawData}");
            report.AppendLine($"Device ID: {DeviceId}");
            report.AppendLine();
#endif
#if USE_FACEBOOK_SDK
            report.AppendLine("==== [Facebook] ====");
            if (FbDataResponse != null)
            {
                report.AppendLine($"FB Data Response: {FbDataResponse}");
                report.AppendLine($"FBId: {FbData.fid};\nFBToken: {FbData.ftok}");
                report.AppendLine($"SDK FBId: {FB.AppId};\nFBToken: {FB.ClientToken}");
            }
            report.AppendLine($"DeepLink: {DeepLink}");
            report.AppendLine($"DeepLink Error: {DeepLinkError}");
            report.AppendLine();
#endif
#if USE_APPSFLYER_SDK || USE_FACEBOOK_SDK
            report.AppendLine("==== [AD] ====");
            report.AppendLine($"AD ID: {AdId}");
            report.AppendLine();
#endif
#if USE_FIREBASE_SDK
            report.AppendLine("==== [Firebase] ====");
            report.AppendLine(string.IsNullOrEmpty(PushToken) ? $"Push Token Failed: {PushTokenFailed}" : $"Push Token: {PushToken}");
            report.AppendLine();
#endif
            ShowDiagnosticsRequested?.Invoke(report.ToString());
            
            AndroidAlert.ShowAlert("Diagnostics", report.ToString(), () =>
            {
                _services.Get<IWebViewService>().LoadUrl(_config.testUrl);
            });
        }

        public void ServicesTimeout(float timeTimeout)
        {
            _servicesTimeout = true;
            _timeTimeout = timeTimeout;
            ChangeState(StartupState.TimedOut);
        }
        
        private void ChangeState(StartupState newState)
        {
            if (CurrentState == newState) 
                return;
            
            CurrentState = newState;
            _completedState = $"{_completedState}; {newState.ToString()}";
            
            if (_config.showLoadingState)
                OnStateChanged?.Invoke(newState);
        }
        
        private void RequestLaunchGame() => LaunchGameRequested?.Invoke();
        
        /// <summary>Computes the expected flags based on current preprocessor symbols.</summary>
        private static InitFlag GetExpectedFlags()
        {
            InitFlag flags = 0;
#if USE_INSTALL_REFERRER
            flags |= InitFlag.InstallReferrer;
#endif
#if USE_FIREBASE_SDK
            flags |= InitFlag.Push;
#endif
#if USE_APPSFLYER_SDK || USE_FACEBOOK_SDK
            flags |= InitFlag.DeepLink;
            flags |= InitFlag.AdId;
#endif
            return flags;
        }
    }
}