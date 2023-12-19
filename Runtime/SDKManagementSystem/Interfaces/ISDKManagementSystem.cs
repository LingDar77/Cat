using System.Collections.Generic;
using SFC.SDKProvider;

namespace SFC.SDKManagementSystem
{
    public interface ISDKManagementSystem
    {
        List<ISDKProvider> Providers { get; }

        List<ProviderType> GetValidProviders<ProviderType>() where ProviderType : ISDKProvider;
    }
}