#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_ANDROID
#define ENABLE_OCULUS
#endif

#if ENABLE_OCULUS
using System.Collections.Generic;
using Oculus.Platform;
using TUI.SDKProvider;
using TUI.Utillities;
using UnityEngine;
using UnityEngine.XR;

namespace TUI.Intergration.OculusSDKProviders
{

    public abstract class OculusBaseSDKProvider : DisableInEdtorScript, ISDKProvider
    {
        public bool IsInitialized { get => Core.IsInitialized(); }
        public bool IsAvailable
        {
            get
            {
                List<XRInputSubsystem> subsystems = new();
                SubsystemManager.GetInstances(subsystems);
                if (subsystems.Count == 0) return false;

                return subsystems[0].subsystemDescriptor.id.Contains("oculus");
            }
        }
        protected virtual void OnEnable()
        {
            if (!Core.IsInitialized())
            {
                Core.AsyncInitialize();
            }
        }
    }
}
#endif