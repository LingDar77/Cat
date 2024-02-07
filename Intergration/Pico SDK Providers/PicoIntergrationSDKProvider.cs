namespace Cat.Intergration.PicoSDKProviders
{
    using System.Collections.Generic;
    using Cat.SDKProvider;
    using Cat.Utillities;
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

                var result = subsystems.Count != 0 && subsystems[0].subsystemDescriptor.id.Contains("PICO", System.StringComparison.OrdinalIgnoreCase);
                return result;
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
        public void Recenter()
        {
            PXR_Plugin.Sensor.UPxr_ResetSensor(ResetSensorOption.ResetRotation);
        }
        private void Start()
        {
            manager = PXR_Manager.Instance;
            PXR_Plugin.System.UPxr_SetConfigInt(ConfigType.UnityLogLevel, 3); //fuck PLog
            manager.lateLatching = true;
            manager.foveationLevel = Unity.XR.PXR.FoveationLevel.TopHigh;
            manager.adaptiveResolution = false; // fuck this shit
            this.Log("Pico sdk initialized.");
        }


    }

}