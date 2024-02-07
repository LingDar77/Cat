#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || EOS
#define EOS_CAN_SHUTDOWN
#endif

#if !EOS_CAN_SHUTDOWN

namespace Cat.Intergration.EOSSDKProviders
{
    using Cat.SDKProvider;
    public partial class EOSIntergrationSDKProvider : UnsupportedSDKBase<EOSIntergrationSDKProvider>
    {
    }
}
#endif