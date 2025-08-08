using System;

namespace LinkModule.Scripts.WebViewService
{
    public interface IWebViewService
    {
        bool IsWebViewOpened();
        event Action OnWebViewClose;
        void Initialize();
        void LoadUrl(string url);
    }
}