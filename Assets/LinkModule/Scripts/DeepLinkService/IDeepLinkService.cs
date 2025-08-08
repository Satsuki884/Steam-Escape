#if USE_FACEBOOK_SDK
using System;

namespace LinkModule.Scripts.DeepLinkService
{
    public interface IDeepLinkService
    {
        event Action<string> OnDeepLinkReceived;
        event Action<string> OnDeeplinkFailed;
        void Initialize();
        void SetFacebookCredentials(string id, string token);
    }
}
#endif