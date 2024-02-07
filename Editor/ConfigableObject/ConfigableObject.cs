#if UNITY_EDITOR
namespace Cat.EditorScript
{
    using System.IO;
    using UnityEditorInternal;
    using UnityEngine;

    public class ConfigableObject<Type> : ScriptableObject where Type : ConfigableObject<Type>
    {
        private static Type instance;
        public static Type Get()
        {
            if (instance != null) return instance;
            var name = typeof(Type).Name;
            var assembly = typeof(Type).Assembly.GetName().Name;

            if (!Directory.Exists($"ProjectSettings/{assembly}"))
            {
                Directory.CreateDirectory($"ProjectSettings/{assembly}");
            }

            var results = InternalEditorUtility.LoadSerializedFileAndForget($"ProjectSettings/{assembly}/{name}.asset");
            if (results != null && results.Length != 0)
            {
                instance = results[0] as Type;
            }
            else
            {
                instance = CreateInstance<Type>();
                InternalEditorUtility.SaveToSerializedFileAndForget(new Type[] { instance }, $"ProjectSettings/{assembly}/{name}.asset", true);
            }

            return instance;
        }

        public static void Save()
        {
            InternalEditorUtility.SaveToSerializedFileAndForget(new Type[] { instance }, $"ProjectSettings/{typeof(Type).Assembly.GetName().Name}/{typeof(Type).Name}.asset", true);
        }

    }
}

#endif