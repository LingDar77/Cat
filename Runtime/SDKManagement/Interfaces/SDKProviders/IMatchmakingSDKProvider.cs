namespace Cat.SDKProvider
{
    public interface IMatchmakingSDKProvider : ISDKProvider
    {
        [System.Serializable]
        public struct Invitation
        {
            public string Destination;
            public string LobbyID;
            public string SessionID;
        }

        /// <summary>
        /// Called when invitation received.
        /// </summary>
        event System.Action<Invitation> OnInvitationRecived;

        /// <summary>
        /// The uniuqe destination to join the player's game.
        /// Note that the destination is the main path to join the game.
        /// </summary>
        string Destination { get; }
        /// <summary>
        /// The possiable lobby id to join the player's game.
        /// Note that there may have no lobby at all.
        /// So, do not use it if you are sure for that.
        /// </summary>
        string LobbyID { get; }
        /// <summary>
        /// The possiable session id to join the player's game.
        /// Note that there may have no session at all.
        /// So, do not use it if you are sure for that.
        /// </summary>
        string SessionID { get; }

        /// <summary>
        /// Set the current lobby ID
        /// </summary>
        /// <param name="lobby"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        void SetCurrentLobby(string lobby, System.Action onSuccess = null, System.Action<string> onFailure = null);
        /// <summary>
        /// Set the current session ID
        /// </summary>
        /// <param name="session"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        void SetCurrentSession(string session, System.Action onSuccess = null, System.Action<string> onFailure = null);
        void SetCurrentDestination(string destination, System.Action onSuccess = null, System.Action<string> onFailure = null);

        /// <summary>
        /// Open the invitation panel to send invitations.
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        void OpenInvitationPresence(System.Action onSuccess = null, System.Action<string> onFailure = null);
    }
}