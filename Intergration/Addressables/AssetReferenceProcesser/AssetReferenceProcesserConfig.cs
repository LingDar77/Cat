#if UNITY_EDITOR 
namespace Cat.Intergration.Addressables.EditorScript
{
    using System.Collections.Generic;
    using System.IO;
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
        public Object[] resolveObjects;
        public FolderReference[] resolveFolders;

        public GroupConfig[] groupConfigs;
        private AddressableAssetSettings settings;
        public void Scan()
        {
            settings = AddressableAssetSettingsDefaultObject.Settings;
            var objects = new List<Object>(resolveObjects);
            objects.AddRange(EditorUtility.CollectDependencies(resolveObjects));

            foreach (var obj in objects)
            {
                TestObject(obj);
            }

            var cnt = 0;
            foreach (var folder in resolveFolders)
            {
                var guids = AssetDatabase.FindAssets("", new string[] { folder.Path });
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!Path.HasExtension(path)) continue;
                    ++cnt;
                    var obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                    TestObject(obj);
                }
            }

            Debug.Log($"Processed {objects.Count + cnt}.");
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
                AssetReferenceProcesserConfig.Get().resolveObjects = null;
            }
            if (GUILayout.Button("Process Assets"))
            {
                AssetReferenceProcesserConfig.Get().Scan();
            }

        }

    }

}


#endif