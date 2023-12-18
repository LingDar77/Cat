#if META_INTERACTION_OVR
using Oculus.Platform;
using SFC.XRSDKProvider;
using UnityEngine;

namespace SFC.Intergration.SDKProviders
{
    public class OculusSDKProvider : MonoBehaviour, IXRSDKProvider
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
            var id = XRPlatformSDKHelper.GetSubsystemID();
            if (id == null || !id.Contains("oculus")) return;
            XRPlatformSDKHelper.SDKProvider = this;
            Debug.Log("using oculus sdk.");

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
            OVRPlugin.suggestedCpuPerfLevel = OVRPlugin.ProcessorPerformanceLevel.SustainedLow;
            OVRPlugin.suggestedGpuPerfLevel = OVRPlugin.ProcessorPerformanceLevel.Boost;
            Debug.Log("oculus sdk initialized.");
        }
    }
}
#endif