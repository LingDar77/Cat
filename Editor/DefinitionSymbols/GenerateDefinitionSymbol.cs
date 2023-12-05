#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
namespace SFC.EditorScript
{
    internal static class GenerateDefinitionSymbol
    {
        [InitializeOnLoadMethod]
        public static void AddDefineSymbols()
        {
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if(defines.Contains("SALTY_FISH_CONTAINER")) return;

            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup), defines + ";SALTY_FISH_CONTAINER");
        }
    }
}
#endif