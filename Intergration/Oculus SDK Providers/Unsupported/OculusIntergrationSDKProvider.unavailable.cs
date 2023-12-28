#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_ANDROID
#define ENABLE_OCULUS
#endif

#if !ENABLE_OCULUS
using SFC.SDKProvider;

namespace SFC.Intergration.OculusSDKProviders
{
    public partial class OculusIntergrationSDKProvider : UnsupportedSDKBase<OculusUsersSDKProvider>
    {

    }
}
#endif