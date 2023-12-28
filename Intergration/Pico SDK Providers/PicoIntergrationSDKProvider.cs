using System.Collections.Generic;
using TUI.SDKProvider;
using TUI.Utillities;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.XR;

public class PicoIntergrationSDKProvider : DisableInEdtorScript, IXRSDKProvider
{
    public bool IsInitialized { get => manager != null; }
    public bool IsAvailable
    {
        get
        {
            List<XRInputSubsystem> subsystems = new();
            SubsystemManager.GetInstances(subsystems);
            if (subsystems.Count == 0) return false;

            return subsystems[0].subsystemDescriptor.id.Contains("PICO");
        }
    }
    private PXR_Manager manager;

    public bool EnableAdaptiveResolution { get => manager.adaptiveResolution; set => manager.adaptiveResolution = value; }
    public int FoveationLevel
    {
        get => (int)manager.foveationLevel; set => manager.foveationLevel = (FoveationLevel)value;
    }
    public bool EnableLateLatching { get => manager.lateLatching; set => manager.lateLatching = value; }
    public int SharpeningLevel { get => (int)manager.sharpeningMode; set => manager.sharpeningMode = (SharpeningMode)value; }


    public event System.Action OnRecenterSuccessed
    {
        add => PXR_Plugin.System.RecenterSuccess += value;
        remove => PXR_Plugin.System.RecenterSuccess -= value;
    }
    private void Start()
    {
        manager = PXR_Manager.Instance;
        manager.lateLatching = true;
        manager.foveationLevel = Unity.XR.PXR.FoveationLevel.TopHigh;
        manager.adaptiveResolution = true;
        manager.adaptiveResolutionPowerSetting = AdaptiveResolutionPowerSetting.HIGH_QUALITY;
        manager.maxEyeTextureScale = .5f;

        Debug.Log("pico sdk initialized.");
    }
}
