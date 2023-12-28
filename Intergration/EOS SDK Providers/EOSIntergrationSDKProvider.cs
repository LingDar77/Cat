#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || EOS
#define EOS_CAN_SHUTDOWN
#endif

#if EOS_CAN_SHUTDOWN
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices;
using TUI.SDKProvider;
using TUI.Utillities;
using UnityEngine;

namespace TUI.Intergration.EOSSDKProviders
{
    public partial class EOSIntergrationSDKProvider : DisableInEdtorScript, ISDKProvider
    {
        [HideInInspector] public EOSManager Manager;
        [HideInInspector] public EpicAccountId LocalUserId;
        public bool IsInitialized { get => Manager != null && loggin; }
        public bool IsAvailable { get; } = true;


        protected bool loggin = false;


        protected virtual void OnEnable()
        {
            if (Manager == null)
            {
                Manager = gameObject.AddComponent<EOSManager>();
            }
            EOSManager.Instance.StartPersistentLogin(e =>
            {
                if (e.ResultCode == Result.Success)
                {
                    LocalUserId = e.LocalUserId;
                    loggin = true;
                    return;
                }
                EOSManager.Instance.StartLoginWithLoginTypeAndToken(Epic.OnlineServices.Auth.LoginCredentialType.AccountPortal, ExternalCredentialType.Epic, null, null, e =>
                {
                    if (e.LocalUserId == null) return;
                    LocalUserId = e.LocalUserId;
                    loggin = true;
                });

            });
        }
    }
}
#endif