#if UNITY_EDITOR
namespace TUI.Intergration.Addressables.EditorScript
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.AddressableAssets.Initialization;
    using UnityEngine.ResourceManagement.Util;


    [CreateAssetMenu(fileName = "PlatformBasedInitializationSettings", menuName = "Addressables/Initialization/Platform-Based Initialization Settings", order = 0)]
    public class PlatformBasedInitializationSettings : ScriptableObject, IObjectInitializationDataProvider
    {
        [System.Serializable]
        public class PlatformCacheInitializationData
        {
            public BuildTarget target;
            public bool CompressionEnabled;
            public string CacheDirectoryOverride;
            public bool LimitCacheSize;
            public long MaximumCacheSize;

            public CacheInitializationData GetData()
            {
                return new CacheInitializationData
                {
                    MaximumCacheSize = MaximumCacheSize,
                    LimitCacheSize = LimitCacheSize,
                    CacheDirectoryOverride = CacheDirectoryOverride,
                    CompressionEnabled = CompressionEnabled
                };
            }
        }
        public string Name { get { return "Asset Bundle Cache Settings Switched by Platform Setting"; } }
        public List<PlatformCacheInitializationData> settings;
        [Header("Default Setting")]
        public bool CompressionEnabled;
        public string CacheDirectoryOverride;
        public bool LimitCacheSize;
        public long MaximumCacheSize;

        public ObjectInitializationData CreateObjectInitializationData()
        {
#if UNITY_EDITOR
            return ObjectInitializationData.CreateSerializedInitializationData<CacheInitialization>(typeof(CacheInitialization).Name, new CacheInitializationData
            {
                MaximumCacheSize = MaximumCacheSize,
                LimitCacheSize = LimitCacheSize,
                CacheDirectoryOverride = CacheDirectoryOverride,
                CompressionEnabled = CompressionEnabled
            });
#else
            if (settings == null || settings.Count == 0)
            {
                return ObjectInitializationData.CreateSerializedInitializationData<CacheInitialization>(typeof(CacheInitialization).Name, new CacheInitializationData
                {
                    MaximumCacheSize = MaximumCacheSize,
                    LimitCacheSize = LimitCacheSize,
                    CacheDirectoryOverride = CacheDirectoryOverride,
                    CompressionEnabled = CompressionEnabled
                });
            }
            var target = EditorUserBuildSettings.activeBuildTarget;
            var setting = settings.Find(s => s.target == target);
            return ObjectInitializationData.CreateSerializedInitializationData<CacheInitialization>(typeof(CacheInitialization).Name, setting.GetData());
#endif

        }
    }
}

#endif