using System.Collections.Generic;
using System.Linq;
using SFC.SDKProvider;
using UnityEngine;
namespace SFC.SDKManagementSystem
{
    public class BuiltinSDKManagement : MonoBehaviour, ISDKManagementSystem, ISingletonSystem<BuiltinSDKManagement>
    {
        [ImplementedInterface(typeof(ISDKProvider))]
        [SerializeField] protected List<MonoBehaviour> ProviderObjects;
        public HashSet<ISDKProvider> Providers { get; set; } = new();
        protected Dictionary<System.Type, ISDKProvider[]> providerCaches = new();

        protected virtual void OnEnable()
        {
            if (ISingletonSystem<BuiltinSDKManagement>.Singleton != null) return;

            ISingletonSystem<BuiltinSDKManagement>.Singleton = this;
            DontDestroyOnLoad(transform.root.gameObject);
            foreach (var provider in ProviderObjects)
            {
                Providers.Add(provider as ISDKProvider);
            }
        }

        protected virtual void OnDisable()
        {
            if (ISingletonSystem<BuiltinSDKManagement>.Singleton.transform != this) return;
            ISingletonSystem<BuiltinSDKManagement>.Singleton = null;
        }

        public virtual ProviderType[] GetValidProviders<ProviderType>() where ProviderType : ISDKProvider
        {
            var type = typeof(ProviderType);
            if (!providerCaches.ContainsKey(type))
            {
                List<ISDKProvider> result = new();
                foreach (var provider in Providers)
                {
                    if (provider is not ProviderType || !provider.IsAvailable()) continue;

                    result.Add(provider);
                    provider.enabled = true;
                }
                providerCaches.Add(type, result.ToArray());
            }

            return providerCaches[type].Cast<ProviderType>().ToArray();
        }
    }
}