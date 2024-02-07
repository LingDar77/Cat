#if UNITY_EDITOR
namespace Cat.EditorScript
{
    using UnityEditor;


    public class HideScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var fields = serializedObject.targetObject.GetType().GetFields();
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
            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                var type = serializedObject.targetObject.GetType().BaseType;
                var method = type.GetMethod("Save", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                method?.Invoke(null, null);
            }
        }

    }
}
#endif