namespace SFC.SDKProvider
{
    public interface ISocialPresenceSDKProvider : ISDKProvider
    {
        [System.Serializable]
        public class User
        {
            public string NickName;
            public string ID;
            public string AvatarUrl;

        }
        [System.Serializable]
        public class InviteDestination
        {
            public string Message;
            public string LobbySessionId;
            public string MatchSessionId;
        }

        event System.Action<InviteDestination> OnInvited;

        void SetCurrentLobby(string lobby, System.Action onSuccess = null, System.Action<string> onFailure = null);
        void SetCurrentMatch(string match, System.Action onSuccess = null, System.Action<string> onFailure = null);
        void SetCurrentLocation(string location, System.Action onSuccess = null, System.Action<string> onFailure = null);
        void SetCurrentSessionJoinable(bool joinable, System.Action onSuccess = null, System.Action<string> onFailure = null);
        void OpenInviteInterface(System.Action onSuccess = null, System.Action<string> onFailure = null);
        void GetCurrentUser(System.Action<User> onSuccess = null, System.Action<string> onFailure = null);
    }
}