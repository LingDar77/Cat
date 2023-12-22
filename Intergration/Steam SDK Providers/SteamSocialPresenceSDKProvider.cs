#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLE_STEAMWORKS
#endif
#if !DISABLE_STEAMWORKS

using System;
using System.Collections;
using SFC.SDKManagementSystem;
using SFC.SDKProvider;
using SFC.Utillities;
using Steamworks;
using UnityEngine;

namespace SFC.Intergration.SteamSDKProviders
{
    public partial class SteamSocialPresenceSDKProvider : DisableInEdtorScript, ISocialPresenceSDKProvider
    {
        private CSteamID currentLobby = CSteamID.Nil;
        private Callback<LobbyEnter_t> onLobbyEnterCallback;
        private Callback<LobbyCreated_t> onLobbyCreatedCallback;

        public bool IsInitialized
        {
            get
            {
                var providers = ISingletonSystem<BuiltinSDKManagement>.Singleton.GetValidProviders<SteamIntergrationSDKProvider>();
                if (providers.Length == 0) return false;
                return providers[0].IsInitialized;
            }
        }
        public bool IsAvailable { get; } = true;

        public event Action<ISocialPresenceSDKProvider.InviteDestination> OnInvited;

        private void Start()
        {
            onLobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
        }

        private void OnLobbyEnter(LobbyEnter_t param)
        {
            currentLobby = (CSteamID)param.m_ulSteamIDLobby;
            Debug.Log("Current lobby: " + currentLobby);
            this.WaitForSeconds(() =>
            {
                SteamFriends.ActivateGameOverlayInviteDialog((CSteamID)param.m_ulSteamIDLobby);
                Debug.Log("Try Open Invite Interface");
            }, 4);
        }

        public void GetCurrentUser(Action<ISocialPresenceSDKProvider.User> onSuccess = null, Action<string> onFailure = null)
        {
            var id = SteamUser.GetSteamID();
            onSuccess?.Invoke(new()
            {
                NickName = SteamFriends.GetFriendPersonaName(id),
                ID = id.ToString(),
                AvatarUrl = null
            });
        }

        public void OpenInviteInterface(Action onSuccess = null, Action<string> onFailure = null)
        {
            SteamFriends.ActivateGameOverlayInviteDialog(currentLobby);
            onSuccess?.Invoke();
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