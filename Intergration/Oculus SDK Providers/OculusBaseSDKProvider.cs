namespace TUI.Intergration.OculusSDKProviders
{
    using System.Collections.Generic;
    using Oculus.Platform;
    using TUI.SDKProvider;
    using TUI.Utillities;
    using UnityEngine;
    using UnityEngine.XR;

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