using System.Collections.Generic;
using SFC.SDKProvider;

namespace SFC.SDKManagementSystem
{
    public interface ISDKManagementSystem : IGameSystem<ISDKManagementSystem>
    {
        HashSet<ISDKProvider> Providers { get; }

        ProviderType[] GetValidProviders<ProviderType>() where ProviderType : ISDKProvider;
    }
}