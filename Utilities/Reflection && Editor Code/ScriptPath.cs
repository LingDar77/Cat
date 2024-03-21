#if UNITY_EDITOR
namespace Cat.Utilities
{
    using System.IO;
    using UnityEditor;


    public class ScriptPath
    {
        public static string GetScriptPath(System.Type type)
        {
            var guids = AssetDatabase.FindAssets(type.Name);
            if (guids == null || guids.Length == 0) return null;

            return Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
#endif