using System;

namespace LinkModule.Scripts.AdService
{
    public interface IAdService
    {
        event Action<string> OnAdIdReceived;
        void Initialize();
    }
}