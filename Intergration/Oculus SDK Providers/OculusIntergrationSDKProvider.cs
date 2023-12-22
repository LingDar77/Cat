using System.Collections.Generic;
using SFC.SDKProvider;
using SFC.Utillities;
using UnityEngine;
using UnityEngine.XR;

namespace SFC.Intergration.OculusSDKProviders
{
    public class OculusIntergrationSDKProvider : DisableInEdtorScript, IXRIntergrationSDKProvider
    {
        private OVRManager manager;

        public bool AdaptiveResolution { get => manager.enableDynamicResolution; set => manager.enableDynamicResolution = value; }
        public int FoveationLevel { get => (int)OVRPlugin.foveatedRenderingLevel; set => OVRPlugin.foveatedRenderingLevel = (OVRPlugin.FoveatedRenderingLevel)value; }
        public bool LateLatching { get => manager.LateLatching; set => manager.LateLatching = value; }
        public int SharpeningLevel
        {
            get => manager.sharpenType == OVRPlugin.LayerSharpenType.None ? 0 : (manager.sharpenType == OVRPlugin.LayerSharpenType.Normal ? 1 : 2);
            set => manager.sharpenType = value == 0 ? OVRPlugin.LayerSharpenType.None : value == 1 ? OVRPlugin.LayerSharpenType.Normal : OVRPlugin.LayerSharpenType.Quality;
        }


        public event System.Action OnRecenterSuccessed;

        private void Start()
        {
            manager = OVRManager.instance;
            if (manager == null)
            {
                manager = gameObject.AddComponent<OVRManager>();
            }
            manager.LateLatching = true;
            manager.enableDynamicResolution = true;
            manager.AllowRecenter = false;
            OVRPlugin.useDynamicFoveatedRendering = true;
            OVRPlugin.foveatedRenderingLevel = OVRPlugin.FoveatedRenderingLevel.HighTop;
            OVRPlugin.suggestedCpuPerfLevel = OVRPlugin.ProcessorPerformanceLevel.PowerSavings;
            OVRPlugin.suggestedGpuPerfLevel = OVRPlugin.ProcessorPerformanceLevel.PowerSavings;
            Debug.Log("oculus sdk initialized.");
        }

        public bool IsAvailable()
        {
            List<XRInputSubsystem> subsystems = new();
            SubsystemManager.GetInstances(subsystems);
            if (subsystems.Count == 0) return false;

            return subsystems[0].subsystemDescriptor.id.Contains("oculus");
        }

        public bool IsInitialized()
        {
            return manager != null;
        }
    }
}
