using System.Collections.Generic;
using System.Linq;
using SFC.SDKProvider;
using UnityEngine;
namespace SFC.SDKManagementSystem
{
    public class BuiltinSDKManagement : MonoBehaviour, ISDKManagementSystem, ISingletonSystem<BuiltinSDKManagement>
    {
#if UNITY_EDITOR
        [SerializeField] private bool AutoCollectProviders = true;
#endif
        [ImplementedInterface(typeof(ISDKProvider))]
        [SerializeField] protected List<MonoBehaviour> ProviderObjects;
        public HashSet<ISDKProvider> Providers { get; set; } = new();
        protected Dictionary<System.Type, ISDKProvider[]> providerCaches = new();
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (!AutoCollectProviders) return;
            var providers = GetComponentsInChildren<ISDKProvider>();
            foreach (var provider in providers)
            {
                if (!ProviderObjects.Contains(provider as MonoBehaviour)) ProviderObjects.Add(provider as MonoBehaviour);
            }
#endif
        }

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
            Debug.Log($"Finded {providerCaches[type].Length} valid providers for {type.Name}.");
            return providerCaches[type].Cast<ProviderType>().ToArray();
        }
    }
}