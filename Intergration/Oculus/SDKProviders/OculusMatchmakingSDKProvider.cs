namespace Cat.Intergration.Oculus.SDKProviders
{
    using System;
    using Oculus.Platform;
    using Cat.SDKProvider;
    using Cat.Utillities;
    using UnityEngine;
    public partial class OculusMatchmakingSDKProvider : OculusBaseSDKProvider, IMatchmakingSDKProvider
    {
        [field: ReadOnlyInEditor]
        [field: SerializeField] public string Destination { get; set; }
        [field: ReadOnlyInEditor]
        [field: SerializeField] public string LobbyID { get; set; }
        [field: ReadOnlyInEditor]
        [field: SerializeField] public string SessionID { get; set; }

        public event Action<IMatchmakingSDKProvider.Invitation> OnInvitationRecived;

        protected override void OnEnable()
        {
            base.OnEnable();
            GroupPresence.SetJoinIntentReceivedNotificationCallback(e =>
            {
                OnInvitationRecived?.Invoke(new()
                {
                    Destination = e.Data.DeeplinkMessage,
                    LobbyID = e.Data.LobbySessionId,
                    SessionID = e.Data.MatchSessionId
                });
            });
        }

        public void OpenInvitationPresence(Action onSuccess = null, Action<string> onFailure = null)
        {
            var options = new InviteOptions();
            GroupPresence.LaunchInvitePanel(options).OnComplete(e =>
            {
                if (e.IsError)
                {
                    onFailure?.Invoke(e.GetError().Message);
                    return;
                }
                onSuccess?.Invoke();
            });
        }

        public void SetCurrentLobby(string lobby, Action onSuccess = null, Action<string> onFailure = null)
        {
            GroupPresence.SetLobbySession(lobby).OnComplete(e =>
            {
                if (e.IsError)
                {
                    onFailure?.Invoke(e.GetError().Message);
                    return;
                }
                LobbyID = lobby;
                onSuccess?.Invoke();
            });
        }

        public void SetCurrentSession(string session, Action onSuccess = null, Action<string> onFailure = null)
        {
            GroupPresence.SetMatchSession(session).OnComplete(e =>
            {
                if (e.IsError)
                {
                    onFailure?.Invoke(e.GetError().Message);
                    return;
                }
                SessionID = session;
                onSuccess?.Invoke();
            });
        }

        public void SetCurrentDestination(string destination, Action onSuccess = null, Action<string> onFailure = null)
        {
            GroupPresence.SetDestination(destination).OnComplete(e =>
            {
                if (e.IsError)
                {
                    onFailure?.Invoke(e.GetError().Message);
                    return;
                }
                Destination = destination;
                onSuccess?.Invoke();
            });
        }
    }
}