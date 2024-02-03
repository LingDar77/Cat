#if UNITY_EDITOR
namespace TUI.EditorScript
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    
    public class ConfigableObject<Type> : ScriptableObject where Type : ConfigableObject<Type>
    {
        private static Type instance;

        public static Type Get()
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
            return "Assets/Settings/TUI/";
        }
    }
}

#endif