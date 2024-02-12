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
            public CacheInitializationData data = new();
        }
        public string Name { get { return "Asset Bundle Cache Settings Switched by Platform Setting"; } }
        public List<PlatformCacheInitializationData> settings;
        private readonly CacheInitializationData data = new();

        public ObjectInitializationData CreateObjectInitializationData()
        {
#if !UNITY_EDITOR
            return ObjectInitializationData.CreateSerializedInitializationData<CacheInitialization>(typeof(CacheInitialization).Name, data);
#else
            var target = EditorUserBuildSettings.activeBuildTarget;
            var setting = settings.Find(s => s.target == target);
            if (settings == null || settings.Count == 0 || setting == null)
            {
                return ObjectInitializationData.CreateSerializedInitializationData<CacheInitialization>(typeof(CacheInitialization).Name, data);
            }

            return ObjectInitializationData.CreateSerializedInitializationData<CacheInitialization>(typeof(CacheInitialization).Name, setting.data);
#endif

        }
    }
}

#endif