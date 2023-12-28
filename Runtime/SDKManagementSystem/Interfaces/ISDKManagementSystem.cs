using System.Collections.Generic;
using TUI.SDKProvider;

namespace TUI.SDKManagementSystem
{
    public interface ISDKManagementSystem : IGameSystem<ISDKManagementSystem>
    {
        HashSet<ISDKProvider> Providers { get; }

        bool AddProvider(ISDKProvider provider);
        bool RemoveProvider(ISDKProvider provider);

        ProviderType[] GetValidProviders<ProviderType>() where ProviderType : ISDKProvider;
    }
}