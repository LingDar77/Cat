using SFC.XRSDKProvider;
using Unity.XR.PXR;
using UnityEngine;

namespace SFC.Intergration.SDKProviders
{
    public class PicoSDKProvider : MonoBehaviour, IXRSDKProvider
    {
        private PXR_Manager manager;

        public bool AdaptiveResolution { get => manager.adaptiveResolution; set => manager.adaptiveResolution = value; }
        public int FoveationLevel
        {
            get => (int)manager.foveationLevel; set => manager.foveationLevel = (FoveationLevel)value;
        }
        public bool LateLatching { get => manager.lateLatching; set => manager.lateLatching = value; }
        public int SharpeningLevel { get => (int)manager.sharpeningMode; set => manager.sharpeningMode = (SharpeningMode)value; }

        public event System.Action OnRecenterSuccessed
        {
            add => PXR_Plugin.System.RecenterSuccess += value;
            remove => PXR_Plugin.System.RecenterSuccess -= value;
        }

        private void Start()
        {
            var id = XRPlatformSDKHelper.GetSubsystemID();
            if (id == null || !id.Contains("PICO")) return;
            XRPlatformSDKHelper.SDKProvider = this;
            Debug.Log("using pico sdk");

            manager = PXR_Manager.Instance;
            manager.lateLatching = true;
            manager.foveationLevel = Unity.XR.PXR.FoveationLevel.TopHigh;
            manager.adaptiveResolution = true;
            manager.adaptiveResolutionPowerSetting = AdaptiveResolutionPowerSetting.HIGH_QUALITY;
            manager.maxEyeTextureScale = .5f;

            Debug.Log("pico sdk initialized.");
        }
    }
}
