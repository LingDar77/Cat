#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || EOS
#define EOS_CAN_SHUTDOWN
#endif

#if !EOS_CAN_SHUTDOWN

namespace TUI.Intergration.EOSSDKProviders
{
    using TUI.SDKProvider;
    public partial class EOSIntergrationSDKProvider : UnsupportedSDKBase<EOSIntergrationSDKProvider>
    {
    }
}
#endif