#if UNITY_EDITOR
namespace Cat.Utillities
{
    using System.IO;
    using UnityEditorInternal;
    using UnityEngine;

    public class ConfigableObject<Type> : ScriptableObject where Type : ConfigableObject<Type>
    {
        private static Type instance;
        public static string SaveFolder = $"ProjectSettings/Cats";
        public static string SavePath = $"{SaveFolder}/{typeof(Type).Name}.neko";

        public static Type Get()
        {
            if (instance != null) return instance;

            if (!Directory.Exists(SaveFolder))
            {
                Directory.CreateDirectory(SaveFolder);
            }

            var results = InternalEditorUtility.LoadSerializedFileAndForget(SavePath);
            if (results != null && results.Length != 0)
            {
                instance = results[0] as Type;
            }
            else
            {
                instance = CreateInstance<Type>();
                InternalEditorUtility.SaveToSerializedFileAndForget(new Type[] { instance }, SavePath, true);
            }

            return instance;
        }

        public static void Save()
        {
            InternalEditorUtility.SaveToSerializedFileAndForget(new Type[] { instance }, SavePath, true);
        }

    }
}

#endif