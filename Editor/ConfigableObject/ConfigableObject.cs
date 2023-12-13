#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SFC.EditorScript
{
    public class ConfigableObject<Type> : ScriptableObject where Type : ConfigableObject<Type>
    {
        private static Type instance;

        public static Type GetConfig()
        {
            if (instance != null) return instance;

            var prototype = CreateInstance<Type>();
            var saveLocation = prototype.GetSaveLocation();
            var path = $"{saveLocation}{typeof(Type).Name}.asset";
            instance = AssetDatabase.LoadAssetAtPath<Type>(path);
            if (instance != null) return instance;
            
            instance = prototype;
            if (!Directory.Exists(saveLocation))
            {
                Directory.CreateDirectory(saveLocation);
                AssetDatabase.Refresh();
            }

            AssetDatabase.CreateAsset(instance, path);
            return instance;
        }

        protected virtual string GetSaveLocation()
        {
            return "Assets/Settings/";
        }
    }
}

#endif