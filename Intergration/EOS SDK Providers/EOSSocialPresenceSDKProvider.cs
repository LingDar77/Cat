#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || EOS
#define EOS_CAN_SHUTDOWN
#endif

#if EOS_CAN_SHUTDOWN
using System;
using Epic.OnlineServices.Presence;
using Epic.OnlineServices.UserInfo;
using PlayEveryWare.EpicOnlineServices;
using SFC.SDKManagementSystem;
using SFC.SDKProvider;
using SFC.Utillities;
using UnityEngine;

namespace SFC.Intergration.EOSSDKProviders
{
    [Obsolete]
    public partial class EOSSocialPresenceSDKProvider : DisableInEdtorScript, ISocialPresenceSDKProvider
    {
        private EOSIntergrationSDKProvider intergration;

        public event Action<ISocialPresenceSDKProvider.InviteDestination> OnInvited;

        public bool IsInitialized
        {
            get
            {
                if (!Init()) return false;
                return intergration.IsInitialized;
            }
        }
        public bool IsAvailable { get; } = true;
        private UserInfoInterface userInfoInterface;
        private PresenceInterface presenceInterface;

        protected virtual void OnEnable()
        {
            Init();
            userInfoInterface = EOSManager.Instance?.GetEOSPlatformInterface()?.GetUserInfoInterface();
            presenceInterface = EOSManager.Instance?.GetEOSPresenceInterface();

            this.WaitUntil(() => IsInitialized, () =>
            {
                var handle = new PresenceModification();
                var data = new PresenceModificationSetDataOptions()
                {
                    Records = new DataRecord[] { }
                };
                handle.SetData(ref data);
                var options = new SetPresenceOptions()
                {
                    LocalUserId = intergration.LocalUserId,
                    PresenceModificationHandle = handle
                };
                presenceInterface.SetPresence(ref options, null,
                (ref SetPresenceCallbackInfo e) =>
                {
                    Debug.Log("Set presence: " + e.ResultCode);
                });
            });

        }
        protected virtual bool Init()
        {
            if (intergration == null)
            {
                var providers = ISingletonSystem<BuiltinSDKManagement>.Singleton.GetValidProviders<EOSIntergrationSDKProvider>();
                if (providers.Length == 0) return false;
                intergration = providers[0];
            }
            return true;
        }

        public void SetCurrentLobby(string lobby, Action onSuccess = null, Action<string> onFailure = null)
        {
        }

        public void SetCurrentMatch(string match, Action onSuccess = null, Action<string> onFailure = null)
        {
        }

        public void SetCurrentLocation(string location, Action onSuccess = null, Action<string> onFailure = null)
        {
        }

        public void SetCurrentSessionJoinable(bool joinable, Action onSuccess = null, Action<string> onFailure = null)
        {
        }

        public void OpenInviteInterface(Action onSuccess = null, Action<string> onFailure = null)
        {
        }

        public void GetCurrentUser(Action<ISocialPresenceSDKProvider.User> onSuccess = null, Action<string> onFailure = null)
        {
            QueryUserInfoOptions options = new()
            {
                LocalUserId = EOSManager.Instance.GetLocalUserId(),
                TargetUserId = intergration.LocalUserId
            };
            userInfoInterface.QueryUserInfo(ref options, null,
            (ref QueryUserInfoCallbackInfo data) =>
            {
                if (data.ResultCode != Epic.OnlineServices.Result.Success)
                {
                    onFailure.Invoke($"Error calling QueryUserInfo: {data.ResultCode}");
                    return;
                }
                CopyUserInfoOptions options = new()
                {
                    LocalUserId = data.LocalUserId,
                    TargetUserId = data.TargetUserId
                };
                userInfoInterface.CopyUserInfo(ref options, out UserInfoData? userInfo);
                onSuccess?.Invoke(new()
                {
                    NickName = userInfo.Value.DisplayName,
                    ID = intergration.LocalUserId.ToString()
                });
            });

        }
    }
}
#endif