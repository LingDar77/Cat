#if UNITY_EDITOR
namespace Cat.Utillities
{
    using UnityEditor;
    using UnityEngine;

    public class ConfigWindow<ConfigType> : EditorWindow where ConfigType : ConfigableObject<ConfigType>
    {
        protected Vector2 scrollPosition;
        protected Editor editor;
        protected virtual void OnEnable()
        {
            editor = Editor.CreateEditor(ConfigableObject<ConfigType>.Get(), typeof(HideScriptEditor));
        }

        protected virtual void Display()
        {

        }
        protected virtual void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.Separator();
            editor.OnInspectorGUI();
            EditorGUILayout.Separator();
            Display();
            EditorGUILayout.EndScrollView();

        }
    }
}
#endif