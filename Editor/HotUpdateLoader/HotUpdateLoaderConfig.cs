#if UNITY_EDITOR && HYBRIDCLR
namespace Cat.Hotupdate
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Cat.Utillities;
    using HybridCLR.Editor.Settings;
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEditorInternal;
    using UnityEngine;

    public class HotUpdateLoaderConfig : ConfigableObject<HotUpdateLoaderConfig>
    {
        public bool Enabled = false;
        public AddressableAssetGroup ConfigGroup;
        public AddressableAssetGroup AssemblyGroup;
        public AddressableAssetGroup MetadataGroup;

    }


    public class HotUpdateLoaderConfigWindow : ConfigWindow<HotUpdateLoaderConfig>
    {
        private static AssemblyOrder orderAsset;

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

        private static void OnCompilationFinished(object _)
        {
            var config = HotUpdateLoaderConfig.Get();
            var aa = AddressableAssetSettingsDefaultObject.GetSettings(false);
            if (aa == null || config.AssemblyGroup == null || config.ConfigGroup == null || config.MetadataGroup == null || !config.Enabled) return;

            var platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            var settings = HybridCLRSettings.Instance;
            var tempFoler = $"Assets/{Path.GetDirectoryName(settings.outputAOTGenericReferenceFile)}";
            var outDllFolder = $"{Environment.CurrentDirectory}/{settings.hotUpdateDllCompileOutputRootDir}/{platform}";
            var outMetaFolder = $"{Environment.CurrentDirectory}/{settings.strippedAOTDllOutputRootDir}/{platform}";
            var assetDllFolder = $"{tempFoler}/Assemblies/{platform}";
            var assetMetaFolder = $"{tempFoler}/Metadata/{platform}";
            var saveDllFolder = $"{Environment.CurrentDirectory}/{assetDllFolder}";
            var saveMetaFolder = $"{Environment.CurrentDirectory}/{assetMetaFolder}";


            PrepareAssemblyOrder(config, aa, platform, settings, tempFoler);

            CopyDlls(outDllFolder, outMetaFolder, saveDllFolder, saveMetaFolder);

            AddDllsToAA(config, aa, platform, assetDllFolder, assetMetaFolder);
        }

        private static void AddDllsToAA(HotUpdateLoaderConfig config, AddressableAssetSettings aa, string platform, string assetDllFolder, string assetMetaFolder)
        {
            //Add dlls to aa
            foreach (var dll in orderAsset.Assemblies)
            {
                var path = $"{assetDllFolder}/{dll}.bytes";
                var guid = AssetDatabase.AssetPathToGUID(path);
                var entry = aa.CreateOrMoveEntry(guid, config.AssemblyGroup, true);
                entry.SetAddress($"{dll}.dll.{platform}");
                entry.SetLabel("Assembly", true, true);
                entry.SetLabel(platform, true, true);
            }

            foreach (var meta in orderAsset.Metadata)
            {
                var path = $"{assetMetaFolder}/{meta}.bytes";
                var guid = AssetDatabase.AssetPathToGUID(path);
                var entry = aa.CreateOrMoveEntry(guid, config.MetadataGroup, true);
                entry.SetAddress($"{meta}.metadata.{platform}");
                entry.SetLabel("Metadata", true, true);
                entry.SetLabel(platform, true, true);
            }

            EditorUtility.SetDirty(config.MetadataGroup);
            EditorUtility.SetDirty(config.AssemblyGroup);
        }

        private static void CopyDlls(string outDllFolder, string outMetaFolder, string saveDllFolder, string saveMetaFolder)
        {
            //Copy assemblies and metadata
            if (!Directory.Exists($"{saveDllFolder}"))
            {
                Directory.CreateDirectory($"{saveDllFolder}");
            }

            if (!Directory.Exists($"{saveMetaFolder}"))
            {
                Directory.CreateDirectory($"{saveMetaFolder}");
            }

            foreach (var dll in orderAsset.Assemblies)
            {
                var path = $"{outDllFolder}/{dll}.dll";
                var target = $"{saveDllFolder}/{dll}.bytes";
                File.Copy(path, target, true);
            }

            foreach (var meta in orderAsset.Metadata)
            {
                var path = $"{outMetaFolder}/{meta}.dll";
                var target = $"{saveMetaFolder}/{meta}.bytes";
                File.Copy(path, target, true);
            }

            AssetDatabase.Refresh();
        }

        private static void PrepareAssemblyOrder(HotUpdateLoaderConfig config, AddressableAssetSettings aa, string platform, HybridCLRSettings settings, string tempFoler)
        {
            //Prepare assembly order
            var assetConfigPath = $"{tempFoler}/AssemblyOrder.{platform}.asset";
            orderAsset = AssetDatabase.LoadAssetAtPath<AssemblyOrder>(assetConfigPath);
            if (orderAsset == null)
            {
                orderAsset = CreateInstance<AssemblyOrder>();
                AssetDatabase.CreateAsset(orderAsset, assetConfigPath);
                AssetDatabase.Refresh();
            }

            var guid = AssetDatabase.AssetPathToGUID(assetConfigPath);

            var entry = aa.CreateOrMoveEntry(guid, config.ConfigGroup, true);
            entry.SetAddress($"AssemblyOrder.{platform}");
            entry.SetLabel("Config", true, true);
            entry.SetLabel(platform, true, true);

            //Add assemblies
            orderAsset.Assemblies.Clear();
            orderAsset.Assemblies.AddRange(settings.hotUpdateAssemblies);
            orderAsset.Assemblies.AddRange(GetSortedAssemblies(settings.hotUpdateAssemblyDefinitions));

            orderAsset.Assemblies = orderAsset.Assemblies.Distinct().ToList();

            if (orderAsset.Assemblies.Contains("Assembly-CSharp"))
            {
                orderAsset.Assemblies.Remove("Assembly-CSharp");
                orderAsset.Assemblies.Add("Assembly-CSharp");
            }

            //Add metadata
            orderAsset.Metadata.Clear();
            var dlls = Regex.Matches(AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/{settings.outputAOTGenericReferenceFile}").ToString(), "\"(.+).dll\"");
            if (dlls.Count != 0)
            {
                orderAsset.Metadata.Clear();
                for (int i = 0; i != dlls.Count; ++i)
                {
                    orderAsset.Metadata.Add(dlls[i].Groups[1].Value);
                }
            }

            EditorUtility.SetDirty(orderAsset);
            EditorUtility.SetDirty(config.ConfigGroup);
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
                    if (matches[i].Groups.Count <= 1) continue;
                    references.Add(matches[i].Groups[1].Value);
                }
                if (map.ContainsKey(guid)) continue;
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


    }
}
#endif
