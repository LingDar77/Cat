namespace SFC.SDKProvider
{
    public interface IXRSDKProvider : ISDKProvider
    {
        /// <summary>
        /// Called when the XR device is successfully recentered.
        /// </summary>
        event System.Action OnRecenterSuccessed;

        bool EnableAdaptiveResolution { get; set; }
     
        bool EnableLateLatching { get; set; }
     
        int FoveationLevel { get; set; }
     
        int SharpeningLevel { get; set; }
    }
}