#if UNITY_EDITOR 
namespace Cat.Intergration.Addressables.EditorScript
{
    using System.Collections.Generic;
    using Cat.Utillities;
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEngine;

    [System.Serializable]
    public class GroupConfig
    {
        public AddressableAssetGroup targetGroup;
        public GroupManagementStrategyBase strategy;

    }
    public class AssetReferenceProcesserConfig : ConfigableObject<AssetReferenceProcesserConfig>
    {
        public Object[] targets;
        public GroupConfig[] groupConfigs;
        private AddressableAssetSettings settings;
        public void Scan()
        {
            settings = AddressableAssetSettingsDefaultObject.Settings;
            var objects = new List<Object>(targets);
            objects.AddRange(EditorUtility.CollectDependencies(targets));
            foreach (var obj in objects)
            {
                TestObject(obj);
            }
            EditorUtility.SetDirty(settings);
        }
        private void TestObject(Object obj)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (settings.FindAssetEntry(guid) != null) return;
            if (path == "" || path.EndsWith(".cs") || path.StartsWith("Package") || !path.StartsWith("Assets"))
                return;
            foreach (var groupConfig in groupConfigs)
            {
                var matcher = groupConfig.strategy;
                if (matcher == null || !matcher.Match(path, groupConfig.targetGroup)) continue;
                settings.CreateOrMoveEntry(guid, groupConfig.targetGroup);
                break;
            }
        }
    }

    public class AssetReferenceProcesserConfigWindow : ConfigWindow<AssetReferenceProcesserConfig>
    {

        [MenuItem("Window/Cat/Addressables/Asset Reference Processer")]
        private static void ShowWindow()
        {
            GetWindow<AssetReferenceProcesserConfigWindow>("Asset Reference Processer", true);
        }

        protected override void Display()
        {
            if (GUILayout.Button("Clean Targets"))
            {
                AssetReferenceProcesserConfig.Get().targets = null;
            }
            if (GUILayout.Button("Process Assets"))
            {
                AssetReferenceProcesserConfig.Get().Scan();
            }

        }

    }

}


#endif