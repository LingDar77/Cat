
namespace SFC.SDKProvider
{
    public interface ISDKProvider : IEnabledSetable, ITransformGetable
    {
        bool IsInitialized();
        bool IsAvailable();
    }
}