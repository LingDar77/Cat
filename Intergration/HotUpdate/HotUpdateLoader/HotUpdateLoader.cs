namespace Cat.Intergration.Hotupdate
{

    using System.Collections;
    using System.Reflection;
    using UnityEngine;
    using Cat.Utillities;
    using UnityEngine.AddressableAssets;
    using UnityEngine.Events;
    using System.Threading;

    public class HotUpdateLoader : MonoBehaviour
    {
        public UnityEvent LoadCompleted;

        private IEnumerator Start()
        {
            this.Log("Fetching Assembly Order.");
            var platform = this.GetRuntimePlatform();
            var order = Addressables.LoadAssetAsync<AssemblyOrder>($"AssemblyOrder.{platform}");
            yield return order;
            this.Log("Fetched Assembly Order. Start Loading Assemblies...");

            foreach (var assembly in order.Result.Assemblies)
            {
                this.LogFormat("Loading assembly: {0}", LogType.Log, assembly);
                var asset = Addressables.LoadAssetAsync<TextAsset>($"{assembly}.{platform}.bytes");
                yield return asset;
                var bytes = asset.Result.bytes;
                var thread = new Thread(() => Assembly.Load(bytes))
                {
                    IsBackground = true
                };
                thread.Start();
                yield return new WaitUntil(() => !thread.IsAlive);
                this.LogFormat("Assembly: {0} Loaded.", LogType.Log, assembly);
            }

            this.Log("Assemblies Loaded. Start Paching AOT Assemblies...");
            foreach (var metadata in order.Result.Metadata)
            {
                this.LogFormat("Loading assembly metadata: {0}", LogType.Log, metadata);
                var asset = Addressables.LoadAssetAsync<TextAsset>($"{metadata}.{platform}.metadata.bytes");
                yield return asset;
                var bytes = asset.Result.bytes;
                var thread = new Thread(() => HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(bytes, HybridCLR.HomologousImageMode.SuperSet))
                {
                    IsBackground = true
                };
                thread.Start();
                yield return new WaitUntil(() => !thread.IsAlive);
                this.LogFormat("Assembly metadata: {0} Pached.", LogType.Log, metadata);
            }
            this.Log("AOT Assemblies Patched.");

            yield return CoroutineHelper.GetWaitForSeconds(1f);
            LoadCompleted.Invoke();
        }

    }

}