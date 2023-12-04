
namespace SFC.XRSDKProvider
{
    public interface IXRSDKProvider
    {
        event System.Action OnRecenterSuccessed;
        bool AdaptiveResolution { get; set; }
        int FoveationLevel { get; set; }
        bool LateLatching { get; set; }
        int SharpeningLevel { get; set; }
    }
}
