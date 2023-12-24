#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLE_STEAMWORKS
#endif
#if DISABLE_STEAMWORKS

using SFC.SDKProvider;
using SFC.Utillities;
using UnityEngine;

namespace SFC.Intergration.SteamSDKProviders
{
    public partial class SteamIntergrationSDKProvider : UnsupportedSDKBase<SteamIntergrationSDKProvider>
    {
    }
}
#endif