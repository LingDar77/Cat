#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLE_STEAMWORKS
#endif
#if DISABLE_STEAMWORKS

using Cat.SDKProvider;

namespace Cat.Intergration.SteamSDKProviders
{
    public partial class SteamUsersSDKProvider : UnsupportedSDKBase<SteamUsersSDKProvider>
    {
       
    }
}
#endif