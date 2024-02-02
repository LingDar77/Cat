using System.Collections.Generic;
using TUI.SDKProvider;

namespace TUI.SDKManagementSystem
{
    public interface ISDKManagement : IGameSystem<ISDKManagement>
    {
        HashSet<ISDKProvider> Providers { get; }

        bool AddProvider(ISDKProvider provider);
        bool RemoveProvider(ISDKProvider provider);

        ProviderType GetValidProvider<ProviderType>() where ProviderType : ISDKProvider;
    }
}