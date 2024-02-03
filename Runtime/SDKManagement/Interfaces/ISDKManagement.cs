namespace TUI.SDKManagementSystem
{
    using System.Collections.Generic;
    using TUI.SDKProvider;
    public interface ISDKManagement : IGameSystem<ISDKManagement>
    {
        HashSet<ISDKProvider> Providers { get; }

        bool AddProvider(ISDKProvider provider);
        bool RemoveProvider(ISDKProvider provider);

        ProviderType GetValidProvider<ProviderType>() where ProviderType : ISDKProvider;
    }
}