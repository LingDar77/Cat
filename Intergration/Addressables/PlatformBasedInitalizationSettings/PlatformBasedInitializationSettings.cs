#if UNITY_EDITOR && ADDRESSABLES
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.ResourceManagement.Util;

namespace TUI.Intergration.Addressables.EditorScript
{
    
    [CreateAssetMenu(fileName = "PlatformBasedInitializationSettings", menuName = "Addressables/Initialization/PlatformBasedInitializationSettings", order = 0)]
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

        public ObjectInitializationData CreateObjectInitializationData()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var setting = settings.Find(s => s.target == target);
            return ObjectInitializationData.CreateSerializedInitializationData<CacheInitialization>(typeof(CacheInitialization).Name, setting.GetData());

        }
    }
}

#endif