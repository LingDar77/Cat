#if UNITY_EDITOR
namespace Cat.Intergration.Hybridclr
{
    using System;
    using System.Collections.Generic;
    using Cat.Utillities;
    using HybridCLR.Editor.Settings;
    using UnityEditor;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEditor.Build.Player;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public class HotUpdateLoaderConfig : ConfigableObject<HotUpdateLoaderConfig>
    {
        public bool Enabled = false;
        public AddressableAssetGroup AssemblyGroup;
    }


    public class HotUpdateLoaderConfigWindow : ConfigWindow<HotUpdateLoaderConfig>
    {
        // private Vector2 scrollPosition;


        [MenuItem("Window/Cat/HotUpdate Loader")]
        private static void ShowWindow()
        {
            var window = GetWindow<HotUpdateLoaderConfigWindow>();
            window.titleContent = new() { text = "Hot Update Loader" };
            window.Show();

        }

        [InitializeOnLoadMethod]
        private static void InitializeHook()
        {

            UnityEditor.Compilation.CompilationPipeline.compilationFinished -= OnCompilationFinished;

            if (!HotUpdateLoaderConfig.Get().Enabled) return;

            UnityEditor.Compilation.CompilationPipeline.compilationFinished += OnCompilationFinished;
        }

        private static void OnCompilationFinished(object obj)
        {
            Debug.Log("compliation finished");
        }
    }
}
#endif