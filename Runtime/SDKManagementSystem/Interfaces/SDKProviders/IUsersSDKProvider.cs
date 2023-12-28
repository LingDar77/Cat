using UnityEngine;

namespace SFC.SDKProvider
{
    public interface IUsersSDKProvider : ISDKProvider
    {
        [System.Serializable]
        public class User
        {
            public string NickName;
            public string ID;
            public Texture2D Avatar;
        }

        /// <summary>
        /// Open friends presence if possible.
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        void OpenFriendsPresence(System.Action onSuccess = null, System.Action<string> onFailure = null);

        /// <summary>
        /// Get the local user's infomation.
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        void GetCurrentUser(System.Action<User> onSuccess = null, System.Action<string> onFailure = null);

        /// <summary>
        /// Qurry user infomation by user ID.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        void GetUserByID(string userID, System.Action<User> onSuccess = null, System.Action<string> onFailure = null);

        /// <summary>
        /// Try set the local user's state.
        /// Normally the state can be shown in friends presence.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        void SetUserState(string key, string value, System.Action onSuccess = null, System.Action<string> onFailure = null);
    }
}