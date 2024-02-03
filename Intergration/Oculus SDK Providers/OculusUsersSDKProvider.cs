namespace TUI.Intergration.OculusSDKProviders
{
    using System;
    using Oculus.Platform;
    using Oculus.Platform.Models;
    using TUI.SDKProvider;

    public partial class OculusUsersSDKProvider : OculusBaseSDKProvider, IUsersSDKProvider
    {
        public void GetCurrentUser(Action<IUsersSDKProvider.User> onSuccess = null, Action<string> onFailure = null)
        {
            Users.GetLoggedInUser().OnComplete(e =>
            {
                if (e.IsError)
                {
                    onFailure?.Invoke(e.GetError().Message);
                    return;
                }
                Users.Get(e.GetUser().ID).OnComplete(message =>
                {
                    if (message.IsError)
                    {
                        onFailure?.Invoke(message.GetError().Message);
                        return;
                    }
                    onSuccess?.Invoke(CreateUser(message.GetUser()));
                });
            });
        }

        public void GetUserByID(string userID, Action<IUsersSDKProvider.User> onSuccess = null, Action<string> onFailure = null)
        {
            Users.Get(ulong.Parse(userID))
            .OnComplete(message =>
            {
                if (message.IsError)
                {
                    onFailure?.Invoke(message.GetError().Message);
                    return;
                }
                onSuccess?.Invoke(CreateUser(message.GetUser()));
            });
        }

        public void OpenFriendsPresence(Action onSuccess = null, Action<string> onFailure = null)
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

        public void SetUserState(string key, string value, Action onSuccess = null, Action<string> onFailure = null)
        {
            //TODO
        }

        protected IUsersSDKProvider.User CreateUser(User user)
        {
            return new()
            {
                ID = user.ID.ToString(),
                NickName = user.DisplayName,
                //TODO convert user image to texture
                // Avatar = user.SmallImageUrl
            };

        }

    }
}