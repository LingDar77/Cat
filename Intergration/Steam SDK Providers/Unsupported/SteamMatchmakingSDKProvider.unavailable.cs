#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLE_STEAMWORKS
#endif
#if DISABLE_STEAMWORKS

using TUI.SDKProvider;

namespace TUI.Intergration.SteamSDKProviders
{
    public partial class SteamMatchmakingSDKProvider : UnsupportedSDKBase<SteamMatchmakingSDKProvider>
    {
       
    }
}
#endif