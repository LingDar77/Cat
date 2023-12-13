#if UNITY_EDITOR && ADDRESSABLES
using System.Collections.Generic;
using SFC.EditorScript;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace SFC.Intergration.AA.EditorScript
{

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

        protected override string GetSaveLocation()
        {
            return "Assets/Settings/";
        }

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

    [CustomEditor(typeof(AssetReferenceProcesserConfig))]
    public class AssetReferenceProcesserConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var fields = typeof(AssetReferenceProcesserConfig).GetFields();
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.BeginVertical();
            foreach (var field in fields)
            {
                var prop = serializedObject.FindProperty(field.Name);
                if (prop != null)
                {
                    EditorGUILayout.PropertyField(prop);
                }
            }
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }
    }

    public class AssetReferenceProcesserConfigWindow : EditorWindow
    {
        private Editor editor;
        private Vector2 scrollPosition;

        [MenuItem("Window/Salty Fish Container/Addressables/Asset Reference Processer")]
        private static void ShowWindow()
        {
            GetWindow<AssetReferenceProcesserConfigWindow>("Asset Reference Processer", true);
        }
        private void OnEnable()
        {
            editor = Editor.CreateEditor(AssetReferenceProcesserConfig.GetConfig());
        }
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.Separator();
            editor.OnInspectorGUI();
            EditorGUILayout.Separator();
            if (GUILayout.Button("Clean"))
            {
                AssetReferenceProcesserConfig.GetConfig().targets = null;
            }
            if (GUILayout.Button("Scan"))
            {
                AssetReferenceProcesserConfig.GetConfig().Scan();
            }
            EditorGUILayout.EndScrollView();

        }
    }

}


#endif