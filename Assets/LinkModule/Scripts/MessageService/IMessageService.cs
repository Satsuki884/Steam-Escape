using System;
using Firebase.Messaging;


namespace LinkModule.Scripts.MessageService
{
    public interface IMessageService
    {
#if USE_FIREBASE_SDK
        event Action<string> OnPushTokenReceived;
        event Action<string> OnPushTokenFailed; 
        event Action<FirebaseMessage> OnMessageReceived;
        void Initialize();
#endif
    }
}