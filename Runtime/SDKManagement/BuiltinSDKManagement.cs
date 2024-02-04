namespace TUI.SDKManagementSystem
{
    using System.Collections.Generic;
    using TUI.Attributes;
    using TUI.SDKProvider;
    using UnityEngine;

    [DefaultExecutionOrder(-1000)]
    public class BuiltinSDKManagement : SingletonSystemBase<BuiltinSDKManagement>, ISDKManagement
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
            if (!AutoCollectProviders || ProviderObjects == null) return;
            var providers = GetComponentsInChildren<ISDKProvider>(true);
            ProviderObjects.Clear();
            foreach (var provider in providers)
            {
                ProviderObjects.Add(provider as MonoBehaviour);
            }
#endif
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (ProviderObjects == null) return;
            foreach (var obj in ProviderObjects)
            {
                if (obj == null) continue;
                Providers.Add((ISDKProvider)obj);
            }
        }
        private void Start()
        {
            foreach (var provider in Providers)
            {
                if (provider != null && provider.transform.gameObject.activeSelf && provider.IsAvailable && !provider.enabled)
                {
                    provider.enabled = true;
                }
            }
        }
        public bool AddProvider(ISDKProvider provider)
        {
            return Providers.Add(provider);
        }

        public bool RemoveProvider(ISDKProvider provider)
        {
            return Providers.Remove(provider);
        }

        public ProviderType GetValidProvider<ProviderType>() where ProviderType : ISDKProvider
        {
            var type = typeof(ProviderType);
            if (!providerCaches.ContainsKey(type))
            {
                foreach (var provider in Providers)
                {
                    if (provider is not ProviderType || !provider.IsAvailable || !provider.transform.gameObject.activeSelf) continue;
                    if (!provider.enabled) provider.enabled = true;
                    return (ProviderType)provider;
                }
            }
            return default;
        }
    }
}