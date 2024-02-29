#if UNITY_EDITOR
namespace Cat.Intergration.Hotupdate
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Cat.Utillities;
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEditorInternal;

    public class HotUpdateLoaderConfig : ConfigableObject<HotUpdateLoaderConfig>
    {
        public bool Enabled = false;
        public AddressableAssetGroup AssemblyGroup;
    }


    public class HotUpdateLoaderConfigWindow : ConfigWindow<HotUpdateLoaderConfig>
    {
        private static AssemblyOrder orderAsset;

        [MenuItem("Window/Cat/IL2CPP/HotUpdate Loader")]
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

        private static void OnCompilationFinished(object _)
        {
            var config = HotUpdateLoaderConfig.Get();
            var aa = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if (aa == null || !config.Enabled || config.AssemblyGroup == null) return;

            var tempFolder = HybridCLR.Editor.Settings.HybridCLRSettings.Instance.outputAOTGenericReferenceFile;
            tempFolder = $"Assets/{Path.GetDirectoryName(tempFolder)}";


            ConfigGroup(config, aa, tempFolder);

            CopyCompiledDlls(config, aa, tempFolder);

            UpdateAssemblyOrder();

        }

        private static void UpdateAssemblyOrder()
        {
            //Update AssemblyOrder
            orderAsset.Assemblies.Clear();

            orderAsset.Assemblies.AddRange(HybridCLR.Editor.Settings.HybridCLRSettings.Instance.hotUpdateAssemblies);

            var assemblies = HybridCLR.Editor.Settings.HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions;

            orderAsset.Assemblies.AddRange(GetSortedAssemblies(assemblies));

            if (orderAsset.Assemblies.Find(e => e == "Assembly-CSharp") != null)
            {
                orderAsset.Assemblies.Remove("Assembly-CSharp");
                orderAsset.Assemblies.Add("Assembly-CSharp");
            }

            EditorUtility.SetDirty(orderAsset);
        }

        private static void CopyCompiledDlls(HotUpdateLoaderConfig config, AddressableAssetSettings aa, string tempFolder)
        {
            //Copy compiled dlls
            var dllFolder = HybridCLR.Editor.Settings.HybridCLRSettings.Instance.hotUpdateDllCompileOutputRootDir;
            dllFolder = $"{Environment.CurrentDirectory}/{dllFolder}";
            var tempDllFolder = $"{Environment.CurrentDirectory}/{tempFolder}/Assemblies";

            if (!Directory.Exists(tempDllFolder))
            {
                Directory.CreateDirectory(tempDllFolder);
            }

            foreach (var dll in HybridCLR.Editor.Settings.HybridCLRSettings.Instance.hotUpdateAssemblies)
            {
                var dllpath = $"{dllFolder}/{EditorUserBuildSettings.activeBuildTarget}/{dll}.dll";
                if (File.Exists(dllpath))
                {
                    File.Copy(dllpath, $"{tempDllFolder}/{dll}.{EditorUserBuildSettings.activeBuildTarget}.bytes", true);
                }
            }

            foreach (var dll in HybridCLR.Editor.Settings.HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions)
            {
                var dllpath = $"{dllFolder}/{EditorUserBuildSettings.activeBuildTarget}/{dll.name}.dll";
                if (File.Exists(dllpath))
                {
                    File.Copy(dllpath, $"{tempDllFolder}/{dll.name}.{EditorUserBuildSettings.activeBuildTarget}.bytes", true);
                }
            }

            AssetDatabase.Refresh();

            //Add to aa

            foreach (var dll in HybridCLR.Editor.Settings.HybridCLRSettings.Instance.hotUpdateAssemblies)
            {
                var guid = AssetDatabase.AssetPathToGUID($"{tempFolder}/Assemblies/{dll}.{EditorUserBuildSettings.activeBuildTarget}.bytes");
                var dllEntry = aa.CreateOrMoveEntry(guid, config.AssemblyGroup, true);
                dllEntry.SetAddress($"{dll}.{EditorUserBuildSettings.activeBuildTarget}.bytes");
                dllEntry.SetLabel("assembly", true, true);
            }

            foreach (var dll in HybridCLR.Editor.Settings.HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions)
            {
                var guid = AssetDatabase.AssetPathToGUID($"{tempFolder}/Assemblies/{dll.name}.{EditorUserBuildSettings.activeBuildTarget}.bytes");
                var dllEntry = aa.CreateOrMoveEntry(guid, config.AssemblyGroup, true);
                dllEntry.SetAddress($"{dll.name}.{EditorUserBuildSettings.activeBuildTarget}.bytes");
                dllEntry.SetLabel("assembly", true, true);
            }
        }

        private static string[] GetSortedAssemblies(AssemblyDefinitionAsset[] assemblies)
        {
            var map = new Dictionary<string, List<string>>();
            var roots = new List<string>();
            var sorted = new List<string>();
            foreach (var definitionAsset in assemblies)
            {
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(definitionAsset));
                var matches = Regex.Matches(definitionAsset.ToString(), "\"GUID:(.+)\"");
                var references = new List<string>();
                for (int i = 0; i != matches.Count; ++i)
                {
                    references.Add(matches[i].Groups[1].Value);
                }
                map.Add(guid, references);
            }


            //find roots
            foreach (var guid in map.Keys)
            {
                bool isRoot = true;
                foreach (var item in map)
                {
                    if (item.Value.Contains(guid))
                    {
                        isRoot = false;
                        break;
                    }
                }
                if (isRoot)
                {
                    roots.Add(guid);
                }
            }

            foreach (var root in roots)
            {
                List<string> result = new();
                Queue<string> queue = new();
                queue.Enqueue(root);
                while (queue.Count != 0)
                {
                    var current = queue.Dequeue();
                    if (!map.ContainsKey(current)) continue;
                    result.Add(current);
                    foreach (var child in map[current])
                    {
                        if (!map.ContainsKey(child)) continue;
                        queue.Enqueue(child);
                    }
                }
                result.Reverse();
                sorted.AddRange(result);
            }

            var results = new List<string>();
            foreach (var sortedItem in sorted)
            {
                var asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(AssetDatabase.GUIDToAssetPath(sortedItem));
                results.Add(asset.name);
            }

            return results.Distinct().ToArray();
        }

        private static void ConfigGroup(HotUpdateLoaderConfig config, AddressableAssetSettings aa, string tempFolder)
        {
            string order = null;

            foreach (var entry in config.AssemblyGroup.entries)
            {
                if (entry.address == $"AssemblyOrder.{EditorUserBuildSettings.activeBuildTarget}")
                {
                    order = entry.guid;
                }
            }

            orderAsset = AssetDatabase.LoadAssetAtPath<AssemblyOrder>($"{tempFolder}/AssemblyOrder.{EditorUserBuildSettings.activeBuildTarget}.asset");

            if (order == null)
            {
                if (orderAsset == null)
                {
                    orderAsset = CreateInstance<AssemblyOrder>();
                    AssetDatabase.CreateAsset(orderAsset, $"{tempFolder}/AssemblyOrder.{EditorUserBuildSettings.activeBuildTarget}.asset");

                }
                order = AssetDatabase.AssetPathToGUID($"{tempFolder}/AssemblyOrder.{EditorUserBuildSettings.activeBuildTarget}.asset");
            }

            aa.CreateOrMoveEntry(order, config.AssemblyGroup, true)
            .SetAddress($"AssemblyOrder.{EditorUserBuildSettings.activeBuildTarget}");

            EditorUtility.SetDirty(config.AssemblyGroup);
        }

    }
}
#endif
