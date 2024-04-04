#if UNITY_EDITOR
namespace Cat.Utillities
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using System.Reflection;
    using Type = System.Type;
    using Array = System.Array;
    public static class EditorTools
    {
        [MenuItem("CONTEXT/LODGroup/Set Lods")]
        static void SetLods(MenuCommand command)
        {
            var context = command.context as LODGroup;
            context.RecalculateBounds();
            var lods = context.GetLODs();
            var finded = 0;
            for (int i = 0; i != context.transform.childCount; ++i)
            {
                if (finded >= lods.Length) return;
                var renders = context.transform.GetChild(i).GetComponentsInChildren<Renderer>();
                if (renders == null || renders.Length == 0) continue;
                lods[finded].renderers = renders;
                ++finded;
            }
            context.SetLODs(lods);

        }

        [MenuItem("Window/Cat/Scene/Clear Empty Objects")]
        static void ClearAllEmptyObjects()
        {
            foreach (var obj in Object.FindObjectsOfType<GameObject>())
            {
                var components = obj.GetComponentsInChildren<Component>();
                if (components.All(e => e is Transform))
                {
                    Object.DestroyImmediate(obj);
                }
            }
        }

        [MenuItem("Window/Cat/Gizmos/Disable All Gizmo Icons")]
        static void DisableAllGizmoIconsMenu()
        {
            var Annotation = Type.GetType("UnityEditor.Annotation, UnityEditor");
            var ClassId = Annotation.GetField("classID");
            var ScriptClass = Annotation.GetField("scriptClass");

            Type AnnotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
            var GetAnnotations = AnnotationUtility.GetMethod("GetAnnotations", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            var SetIconEnabled = AnnotationUtility.GetMethod("SetIconEnabled", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            Array annotations = (Array)GetAnnotations.Invoke(null, null);
            foreach (var a in annotations)
            {
                int classId = (int)ClassId.GetValue(a);
                string scriptClass = (string)ScriptClass.GetValue(a);

                SetIconEnabled.Invoke(null, new object[] { classId, scriptClass, 0 });
            }
        }
        [MenuItem("Window/Cat/Gizmos/Enable All Gizmo Icons")]
        static void EnableAllGizmoIconsMenu()
        {
            var Annotation = Type.GetType("UnityEditor.Annotation, UnityEditor");
            var ClassId = Annotation.GetField("classID");
            var ScriptClass = Annotation.GetField("scriptClass");

            Type AnnotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
            var GetAnnotations = AnnotationUtility.GetMethod("GetAnnotations", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            var SetIconEnabled = AnnotationUtility.GetMethod("SetIconEnabled", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            Array annotations = (Array)GetAnnotations.Invoke(null, null);
            foreach (var a in annotations)
            {
                int classId = (int)ClassId.GetValue(a);
                string scriptClass = (string)ScriptClass.GetValue(a);

                SetIconEnabled.Invoke(null, new object[] { classId, scriptClass, 1 });
            }
        }
    }
}

#endif