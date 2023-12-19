using System.Collections.Generic;
using SFC.SDKProvider;
using UnityEngine;
namespace SFC.SDKManagementSystem
{
    public class BuiltinSDKManagement : MonoBehaviour, ISDKManagementSystem
    {
        [ImplementedInterface(typeof(ISDKProvider))]
        [SerializeField] protected List<MonoBehaviour> ProviderObjects;
        public HashSet<ISDKProvider> Providers { get; set; } = new();

        protected void Start()
        {
            foreach (var obj in ProviderObjects)
            {
                var provider = obj as ISDKProvider;
                Providers.Add(provider);
                if (!provider.IsAvailable()) continue;
                provider.enabled = true;
            }
        }
        protected Dictionary<System.Type, HashSet<ISDKProvider>> providerCaches = new();

        public virtual HashSet<ISDKProvider> GetValidProviders<ProviderType>() where ProviderType : ISDKProvider
        {
            if (providerCaches.ContainsKey(typeof(ProviderType)))
            {
                return providerCaches[typeof(ProviderType)];
            }
            HashSet<ISDKProvider> result = new();
            foreach (var provider in Providers)
            {
                if (provider is not ProviderType || !provider.IsAvailable()) continue;

                result.Add(provider);
            }
            providerCaches.Add(typeof(ProviderType), result);
            return result;
        }
    }
}