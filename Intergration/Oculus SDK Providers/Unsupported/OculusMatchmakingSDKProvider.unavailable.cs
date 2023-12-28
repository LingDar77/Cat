#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_ANDROID
#define ENABLE_OCULUS
#endif

#if !ENABLE_OCULUS
using TUI.SDKProvider;
namespace TUI.Intergration.OculusSDKProviders
{
    public partial class OculusMatchmakingSDKProvider : UnsupportedSDKBase<OculusUsersSDKProvider>
    {

    }
}
#endif