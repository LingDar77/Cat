using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace SFC.XRSDKProvider
{
    public static class XRPlatformSDKHelper
    {
        public static IXRSDKProvider SDKProvider;
        private static string subsystemIDCache = null;
        private static XRInputSubsystem subsystemCache;
        public static string GetSubsystemID()
        {
            if (subsystemIDCache != null) return subsystemIDCache;
            var subsystem = GetSubsystem();
            if (subsystem != null)
                subsystemIDCache = subsystem.subsystemDescriptor.id;
            return subsystemIDCache;
        }
        public static XRInputSubsystem GetSubsystem()
        {
            if (subsystemCache != null) return subsystemCache.running ? subsystemCache : null;
            List<XRInputSubsystem> subsystems = new();
            SubsystemManager.GetInstances(subsystems);
            if (subsystems.Count > 0)
                subsystemCache = subsystems[0];
            return subsystemCache;
        }

    }
}