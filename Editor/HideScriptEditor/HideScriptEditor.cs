using UnityEditor;

namespace SFC.EditorScript
{
    public class HideScriptEditor<Type> : Editor
    {
        public override void OnInspectorGUI()
        {
            var fields = typeof(Type).GetFields();
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
}