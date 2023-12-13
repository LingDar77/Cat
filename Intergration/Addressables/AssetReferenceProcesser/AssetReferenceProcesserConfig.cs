#if UNITY_EDITOR && ADDRESSABLES
using SFC.EditorScript;
using UnityEditor;

namespace SFC.Intergration.AA.EditorScript
{

    public class AssetReferenceProcesserConfig : ConfigableObject<AssetReferenceProcesserConfig>
    {
        public string asd;
        AssetReferenceProcesserConfig()
        {
            SaveLocation = "Assets/Settings/SaltyFishContaner/Addressables/";
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
            editor.OnInspectorGUI();
        }
    }
}


#endif