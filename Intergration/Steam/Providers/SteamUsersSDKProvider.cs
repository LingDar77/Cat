namespace Cat.Intergration.Steam.SDKProvider
{
    using Cat.SDKProvider;

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX
    using System;
    using Cat.Utilities;
    using Steamworks;
    using UnityEngine;
    public partial class SteamUsersSDKProvider : DisableInEdtorScript, IUsersSDKProvider
    {
        [SerializeField] private SteamIntergrationSDKProvider intergration;

        public bool IsInitialized { get => intergration.IsInitialized; }
        public bool IsAvailable { get => intergration.IsAvailable; }

        private void OnEnable()
        {
            if (!intergration.IsAvailable || !intergration.IsInitialized)
            {
                enabled = false;
                Debug.Log("The SDK is not available before SteamIntergrationSDKProvider full initialized.");
            }
        }

        public void GetCurrentUser(Action<IUsersSDKProvider.User> onSuccess = null, Action<string> onFailure = null)
        {
            var id = SteamUser.GetSteamID();
            onSuccess?.Invoke(new()
            {
                NickName = SteamFriends.GetFriendPersonaName(id),
                ID = id.ToString(),
                Avatar = null
            });
        }

        public void GetUserByID(string userID, Action<IUsersSDKProvider.User> onSuccess = null, Action<string> onFailure = null)
        {
            onSuccess?.Invoke(new()
            {
                NickName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(userID)),
                ID = userID,
                Avatar = null
            });
        }

        public void SetUserState(string key, string value, Action onSuccess = null, Action<string> onFailure = null)
        {
            if (!SteamFriends.SetRichPresence(key, value))
            {
                onFailure?.Invoke("Set state failed");
                return;
            }
            onSuccess?.Invoke();
        }

        public void OpenFriendsPresence(Action onSuccess = null, Action<string> onFailure = null)
        {
            SteamFriends.ActivateGameOverlay("Friends");
        }
    }
#else
    public partial class SteamUsersSDKProvider : UnsupportedSDKBase<SteamUsersSDKProvider>
    {

    }
#endif
}