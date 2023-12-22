using System.Collections.Generic;
using System.Linq;
using SFC.SDKProvider;
using UnityEngine;
namespace SFC.SDKManagementSystem
{
    public class BuiltinSDKManagement : SingletonSystemBase<BuiltinSDKManagement>, ISDKManagementSystem
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

        protected override void OnEnable()
        {
            base.OnEnable();
            Providers = ProviderObjects.Cast<ISDKProvider>().ToHashSet();

        }
        private void Start()
        {
            foreach (var provider in Providers)
            {
                if (provider != null && !provider.IsAvailable) continue;
                provider.enabled = true;
            }
        }
        public virtual ProviderType[] GetValidProviders<ProviderType>() where ProviderType : ISDKProvider
        {
            var type = typeof(ProviderType);
            if (!providerCaches.ContainsKey(type))
            {
                List<ISDKProvider> result = new();
                foreach (var provider in Providers)
                {
                    if (provider is not ProviderType || !provider.IsAvailable) continue;
                    result.Add(provider);
                }
                providerCaches.Add(type, result.ToArray());
            }
            Debug.Log($"Finded {providerCaches[type].Length} valid providers for {type.Name}.");
            return providerCaches[type].Cast<ProviderType>().ToArray();
        }
    }
}