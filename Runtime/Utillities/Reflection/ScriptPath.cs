using System.IO;
using UnityEditor;

namespace Cat.Utillities
{
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