#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLE_STEAMWORKS
#endif
#if DISABLE_STEAMWORKS

using System;
using SFC.SDKProvider;
using SFC.Utillities;
using UnityEngine;

namespace SFC.Intergration.SteamSDKProviders
{
    public partial class SteamSocialPresenceSDKProvider : DisableInEdtorScript, ISocialPresenceSDKProvider
    {
        public bool IsInitialized { get; }
        public bool IsAvailable { get; }

        public event Action<ISocialPresenceSDKProvider.InviteDestination> OnInvited;
        private void Start()
        {
            Debug.LogWarning("Steam Social Presence SDK Provider is not supported on this platform.");
            enabled = false;
        }
        public void GetCurrentUser(Action<ISocialPresenceSDKProvider.User> onSuccess = null, Action<string> onFailure = null)
        {
        }

        public void OpenInviteInterface(Action onSuccess = null, Action<string> onFailure = null)
        {
        }

        public void SetCurrentLobby(string lobby, Action onSuccess = null, Action<string> onFailure = null)
        {
        }

        public void SetCurrentLocation(string location, Action onSuccess = null, Action<string> onFailure = null)
        {
        }

        public void SetCurrentMatch(string match, Action onSuccess = null, Action<string> onFailure = null)
        {
        }

        public void SetCurrentSessionJoinable(bool joinable, Action onSuccess = null, Action<string> onFailure = null)
        {
        }
    }
}
#endif