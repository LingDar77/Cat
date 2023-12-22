
namespace SFC.SDKProvider
{
    public interface ISDKProvider : IEnabledSetable, ITransformGetable
    {
        bool IsInitialized { get; }
        bool IsAvailable { get; }
    }
}