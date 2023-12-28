using System;
using TUI.SDKProvider;
using TUI.Utillities;
using Steamworks;
using UnityEngine;
namespace TUI.Intergration.SteamSDKProviders
{

    public class SteamMatchmakingSDKProvider : DisableInEdtorScript, IMatchmakingSDKProvider
    {
        [SerializeField] private SteamIntergrationSDKProvider intergration;
        [SerializeField] private int DefaultLobbyPlayers = 4;

        public bool IsInitialized { get => intergration.IsInitialized && currentLobby != CSteamID.Nil; }
        public bool IsAvailable { get => intergration.IsAvailable; }
        public string Destination { get; set; }
        public string LobbyID { get; set; }
        public string SessionID { get; set; }
        private CSteamID currentLobby = CSteamID.Nil;

        public event Action<IMatchmakingSDKProvider.Invitation> OnInvitationRecived;
        private Callback<LobbyEnter_t> onLobbyEnterCallback;
        private Callback<GameRichPresenceJoinRequested_t> onJoinRequested;
        private Callback<GameLobbyJoinRequested_t> onJoinLobbyRequested;

        private void OnEnable()
        {
            onLobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
            onJoinRequested = Callback<GameRichPresenceJoinRequested_t>.Create(OnJoinRequested);
            onJoinLobbyRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinLobbyRequested);
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, DefaultLobbyPlayers);
        }

        private void OnJoinLobbyRequested(GameLobbyJoinRequested_t param)
        {
            Debug.Log($"Join lobby requested: {param.m_steamIDFriend} {param.m_steamIDLobby}");
            OnInvitationRecived?.Invoke(new()
            {
                Destination = param.m_steamIDLobby.ToString(),
                LobbyID = param.m_steamIDLobby.ToString()
            });
        }

        private void OnDisable()
        {
            onLobbyEnterCallback.Unregister();
            onJoinRequested.Unregister();
        }

        private void OnLobbyEnter(LobbyEnter_t param)
        {
            currentLobby = (CSteamID)param.m_ulSteamIDLobby;
            Debug.Log("Current lobby: " + currentLobby);
        }
        private void OnJoinRequested(GameRichPresenceJoinRequested_t param)
        {
            Debug.Log($"Join requested: {param.m_steamIDFriend} {param.m_rgchConnect}");
            OnInvitationRecived?.Invoke(new()
            {
                Destination = param.m_rgchConnect,
            });
        }
        public void OpenInvitationPresence(Action onSuccess = null, Action<string> onFailure = null)
        {
            SteamFriends.ActivateGameOverlayInviteDialog(currentLobby);
            onSuccess?.Invoke();
        }

        public void SetCurrentDestination(string destination, Action onSuccess = null, Action<string> onFailure = null)
        {
            Destination = destination;
        }

        public void SetCurrentLobby(string lobby, Action onSuccess = null, Action<string> onFailure = null)
        {
            LobbyID = lobby;
        }

        public void SetCurrentSession(string session, Action onSuccess = null, Action<string> onFailure = null)
        {
            SessionID = session;
        }
    }
}