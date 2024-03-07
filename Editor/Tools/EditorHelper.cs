#if UNITY_EDITOR
namespace Cat.Utillities
{
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    public static class EditorHelper
    {
        public static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }
        
        public static void DisplayProgressBar(string tips, int progressValue, int totalValue)
        {
            EditorUtility.DisplayProgressBar("Progress", $"{tips} : {progressValue}/{totalValue}", (float)progressValue / totalValue);
        }

        public static object InvokeNonPublicStaticMethod(System.Type type, string method, params object[] parameters)
        {
            var methodInfo = type.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
            if (methodInfo == null)
            {
                UnityEngine.Debug.LogError($"{type.FullName} not found method : {method}");
                return null;
            }
            return methodInfo.Invoke(null, parameters);
        }

        public static object InvokePublicStaticMethod(System.Type type, string method, params object[] parameters)
        {
            var methodInfo = type.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
            {
                UnityEngine.Debug.LogError($"{type.FullName} not found method : {method}");
                return null;
            }
            return methodInfo.Invoke(null, parameters);
        }

        public static void FocusUnityGameWindow()
        {
            System.Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.GameView");
            EditorWindow.GetWindow(T, false, "GameView", true);
        }

        public static void ClearCurrentShaderVariantCollection()
        {
            InvokeNonPublicStaticMethod(typeof(ShaderUtil), "ClearCurrentShaderVariantCollection");
        }

        public static void SaveCurrentShaderVariantCollection(string savePath)
        {
            InvokeNonPublicStaticMethod(typeof(ShaderUtil), "SaveCurrentShaderVariantCollection", savePath);
        }

        public static int GetCurrentShaderVariantCollectionShaderCount()
        {
            return (int)InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionShaderCount");
        }

        public static int GetCurrentShaderVariantCollectionVariantCount()
        {
            return (int)InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionVariantCount");
        }

        public static string GetShaderVariantCount(string assetPath)
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);
            var variantCount = InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetVariantCount", shader, true);
            return variantCount.ToString();
        }
    }
}
#endif