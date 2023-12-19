using System.Collections.Generic;
using SFC.SDKProvider;
using UnityEngine;
namespace SFC.SDKManagementSystem
{
    public class BuiltinSDKManagement : MonoBehaviour, ISDKManagementSystem
    {
        [ImplementedInterface(typeof(ISDKProvider))]
        [SerializeField] private List<Object> ProviderObjects;
        public List<ISDKProvider> Providers { get; }

        public List<ProviderType> GetValidProviders<ProviderType>() where ProviderType : ISDKProvider
        {
            throw new System.NotImplementedException();
        }
    }
}