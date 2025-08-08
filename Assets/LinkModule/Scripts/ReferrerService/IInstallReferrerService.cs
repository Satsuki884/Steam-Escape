using System;
using System.Collections.Generic;

namespace LinkModule.Scripts.ReferrerService
{
    public interface IInstallReferrerService
    {
        event Action<string> OnRawReferrerReceived;
        event Action<Dictionary<string, string>> OnReferrerParsed;
        event Action<int, string> OnFailed;
        void Initialize();
    }
}