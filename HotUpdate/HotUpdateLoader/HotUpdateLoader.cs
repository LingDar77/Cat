namespace Cat.Hotupdate
{

    using System.Collections;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.Events;
    using System.Threading;

    public class HotUpdateLoader : MonoBehaviour
    {
        public UnityEvent LoadCompleted;

#if UNITY_EDITOR
        public static string GetRuntimePlatform()
        {
            return UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
        }
#else
        public static string GetRuntimePlatform()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return "StandaloneWindows64";
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        return "StandaloneOSX";
#elif UNITY_ANDROID
        return "Android";
#elif UNITY_IPHONE
        return "iPhone";
#else
        return "Unknown";
#endif
        }
#endif
        private IEnumerator Start()
        {
            Debug.Log("Fetching Assembly Order.");
            var platform = GetRuntimePlatform();
            var order = Addressables.LoadAssetAsync<AssemblyOrder>($"AssemblyOrder.{platform}");
            yield return order;
            if (order.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                LoadCompleted.Invoke();
                yield break;
            }
            Debug.Log("Fetched Assembly Order. Start Loading Assemblies...");

            foreach (var assembly in order.Result.Assemblies)
            {
                Debug.LogFormat("Loading assembly: {0}", assembly);
                var asset = Addressables.LoadAssetAsync<TextAsset>($"{assembly}.dll.{platform}");
                yield return asset;
                var bytes = asset.Result.bytes;
                var thread = new Thread(() => Assembly.Load(bytes))
                {
                    IsBackground = true
                };
                thread.Start();
                yield return new WaitUntil(() => !thread.IsAlive);
                Debug.LogFormat("Assembly: {0} Loaded.", assembly);
            }

            Debug.Log("Assemblies Loaded. Start Paching AOT Assemblies...");
            foreach (var metadata in order.Result.Metadata)
            {
                Debug.LogFormat("Loading assembly metadata: {0}", metadata);
                var asset = Addressables.LoadAssetAsync<TextAsset>($"{metadata}.metadata.{platform}");
                yield return asset;
                var bytes = asset.Result.bytes;
                var thread = new Thread(() => HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(bytes, HybridCLR.HomologousImageMode.SuperSet))
                {
                    IsBackground = true
                };
                thread.Start();
                yield return new WaitUntil(() => !thread.IsAlive);
                Debug.LogFormat("Assembly metadata: {0} Pached.", metadata);
            }
            Debug.Log("AOT Assemblies Patched.");

            yield return new WaitForSeconds(1);
            LoadCompleted.Invoke();
        }

    }

}