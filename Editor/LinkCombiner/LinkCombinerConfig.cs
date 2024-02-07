#if UNITY_EDITOR
namespace Cat.EditorScript.LinkCombiner
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class LinkCombinerConfig : ConfigableObject<LinkCombinerConfig>
    {
        public List<TextAsset> CombineFiles;
        public TextAsset OutputFile;
    }


    public class LinkCombinerConfigWindow : EditorWindow
    {
        [CustomEditor(typeof(LinkCombinerConfig))]
        class EditorWindow : HideScriptEditor<LinkCombinerConfig> { }

        private Editor editor;
        private Vector2 scrollPosition;

        [MenuItem("Window/TUI/IL2CPP/Link Files Combiner")]
        private static void ShowWindow()
        {
            GetWindow<LinkCombinerConfigWindow>("Asset Reference Processer", true);
        }
        private void OnEnable()
        {
            editor = Editor.CreateEditor(LinkCombinerConfig.Get());
        }
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            editor.OnInspectorGUI();

            EditorGUILayout.EndScrollView();

        }
    }
}
#endif