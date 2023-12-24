#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || EOS
#define EOS_CAN_SHUTDOWN
#endif

#if EOS_CAN_SHUTDOWN
using PlayEveryWare.EpicOnlineServices;
using SFC.SDKProvider;
using SFC.Utillities;

namespace SFC.Intergration.EOSSDKProviders
{
    public partial class EOSIntergrationSDKProvider : DisableInEdtorScript, ISDKProvider
    {
        private EOSManager manager;

        public bool IsInitialized { get => manager != null; }
        public bool IsAvailable { get; } = true;

        protected virtual void OnEnable()
        {
            if (manager == null)
            {
                manager = gameObject.AddComponent<EOSManager>();
            }
        }
    }
}
#endif