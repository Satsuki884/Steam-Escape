using System;

namespace LinkModule.Scripts.DeepLinkService
{
    public interface IAppsFlyerService
    {
        string DeviceId { get; }
        event Action<string> OnCampaignReceived;
        event Action<string> OnCampaignFailed;
        void Initialize();
    }
}