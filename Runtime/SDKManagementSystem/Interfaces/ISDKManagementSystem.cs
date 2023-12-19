using System.Collections.Generic;
using SFC.SDKProvider;

namespace SFC.SDKManagementSystem
{
    public interface ISDKManagementSystem
    {
        HashSet<ISDKProvider> Providers { get; }

        HashSet<ISDKProvider> GetValidProviders<ProviderType>() where ProviderType : ISDKProvider;
    }
}