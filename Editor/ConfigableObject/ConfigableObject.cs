#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SFC.EditorScript
{
    public class ConfigableObject<Type> : ScriptableObject where Type : ScriptableObject
    {
        protected static string SaveLocation = "Assets/Settings/";
        private static Type instance;

        public static Type GetConfig()
        {
            if (instance != null) return instance;
            var path = $"{SaveLocation}{typeof(Type).Name}.asset";
            instance = AssetDatabase.LoadAssetAtPath<Type>(path);
            if (instance != null) return instance;

            if (!Directory.Exists(SaveLocation))
            {
                Directory.CreateDirectory(SaveLocation);
            }
            AssetDatabase.Refresh();
            instance = CreateInstance<Type>();
            AssetDatabase.CreateAsset(instance, path);
            return instance;
        }
    }
}

#endif