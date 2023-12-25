using System.Collections.Generic;
using SFC.SDKProvider;

namespace SFC.SDKManagementSystem
{
    public interface ISDKManagementSystem : IGameSystem<ISDKManagementSystem>
    {
        HashSet<ISDKProvider> Providers { get; }

        bool AddProvider(ISDKProvider provider);
        bool RemoveProvider(ISDKProvider provider);

        ProviderType[] GetValidProviders<ProviderType>() where ProviderType : ISDKProvider;
    }
}