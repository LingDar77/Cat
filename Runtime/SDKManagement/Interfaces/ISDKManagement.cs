namespace Cat.SDKManagementSystem
{
    using System.Collections.Generic;
    using Cat.SDKProvider;
    public interface ISDKManagement : ICatSystem<ISDKManagement>
    {
        HashSet<ISDKProvider> Providers { get; }

        bool AddProvider(ISDKProvider provider);
        bool RemoveProvider(ISDKProvider provider);

        ProviderType GetValidProvider<ProviderType>() where ProviderType : ISDKProvider;
    }
}