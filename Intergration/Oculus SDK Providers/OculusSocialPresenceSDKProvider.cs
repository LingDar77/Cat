using SFC.SDKProvider;
using System.Collections.Generic;
using Oculus.Platform;
using UnityEngine;
using UnityEngine.XR;
using Oculus.Platform.Models;
using System;

namespace SFC.Intergration.OculusSDKProviders
{

    public class OculusSocialPresenceSDKProvider : MonoBehaviour, ISocialPresenceSDKProvider
    {
        public event Action<ISocialPresenceSDKProvider.InviteDestination> OnInvited;
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (enabled) enabled = false;
        }
#endif
        public bool IsAvailable()
        {
            List<XRInputSubsystem> subsystems = new();
            SubsystemManager.GetInstances(subsystems);
            if (subsystems.Count == 0) return false;

            return subsystems[0].subsystemDescriptor.id.Contains("oculus");
        }
        public bool IsInitialized()
        {
            return Core.IsInitialized();
        }
        private void Start()
        {
            if (!Core.IsInitialized())
            {
                Core.AsyncInitialize();
            }
            GroupPresence.SetJoinIntentReceivedNotificationCallback(e =>
            {
                if (e.IsError) return;
                OnInvited?.Invoke(new()
                {
                    Message = e.Data.DeeplinkMessage,
                    LobbySessionId = e.Data.LobbySessionId,
                    MatchSessionId = e.Data.MatchSessionId
                });
            });
        }

        public void GetCurrentUser(Action<ISocialPresenceSDKProvider.User> onSuccess = null, Action<string> onFailure = null)
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

        public void SetCurrentLobby(string lobby, Action onSuccess = null, Action<string> onFailure = null)
        {
            GroupPresence.SetLobbySession(lobby).OnComplete(e =>
            {
                if (e.IsError)
                {
                    onFailure?.Invoke(e.GetError().Message);
                    return;
                }
                onSuccess?.Invoke();
            });
        }

        public void SetCurrentMatch(string match, Action onSuccess = null, Action<string> onFailure = null)
        {
            GroupPresence.SetMatchSession(match).OnComplete(e =>
             {
                 if (e.IsError)
                 {
                     onFailure?.Invoke(e.GetError().Message);
                     return;
                 }
                 onSuccess?.Invoke();
             });
        }

        public void SetCurrentSessionJoinable(bool joinable, Action onSuccess = null, Action<string> onFailure = null)
        {
            GroupPresence.SetIsJoinable(joinable).OnComplete(e =>
            {
                if (e.IsError)
                {
                    onFailure?.Invoke(e.GetError().Message);
                    return;
                }
                onSuccess?.Invoke();
            });
        }
        public void SetCurrentLocation(string location, Action onSuccess = null, Action<string> onFailure = null)
        {
            GroupPresence.SetDestination(location).OnComplete(e =>
            {
                if (e.IsError)
                {
                    onFailure?.Invoke(e.GetError().Message);
                    return;
                }
                onSuccess?.Invoke();
            });
        }
        public void OpenInviteInterface(Action onSuccess = null, Action<string> onFailure = null)
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





        protected ISocialPresenceSDKProvider.User CreateUser(User user)
        {
            return new()
            {
                ID = user.ID.ToString(),
                NickName = user.DisplayName,
                AvatarUrl = user.SmallImageUrl
            };

        }


    }
}