
using SFC.SDKManagementSystem;

namespace SFC.SDKProvider
{
    public interface ISDKProvider : IEnabledSetable, ITransformGetable
    {
        bool IsAvailable();
    }
}