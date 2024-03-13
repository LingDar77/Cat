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
        public UnityEvent<string> OnMessageLog;

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
            OnMessageLog.Invoke("Fetching Assembly Order.");
            var platform = GetRuntimePlatform();
            var order = Addressables.LoadAssetAsync<AssemblyOrder>($"AssemblyOrder.{platform}");
            yield return order;

            if (order.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                LoadCompleted.Invoke();
                yield break;
            }

            for (int i = 0; i != order.Result.Assemblies.Count; ++i)
            {
                var assembly = order.Result.Assemblies[i];
                var asset = Addressables.LoadAssetAsync<TextAsset>($"{assembly}.dll.{platform}");
                yield return asset;
                var bytes = asset.Result.bytes;
                var thread = new Thread(() => Assembly.Load(bytes))
                {
                    IsBackground = true
                };
                thread.Start();
                yield return new WaitUntil(() => !thread.IsAlive);
                OnMessageLog.Invoke($"Assembly: {assembly} Loaded ({i + 1}/{order.Result.Assemblies.Count}).");
                yield return new WaitForSeconds(.1f);
            }

            for (int i = 0; i != order.Result.Metadata.Count; i++)
            {
                var metadata = order.Result.Metadata[i];
                var asset = Addressables.LoadAssetAsync<TextAsset>($"{metadata}.metadata.{platform}");
                yield return asset;
                var bytes = asset.Result.bytes;
                var thread = new Thread(() => HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(bytes, HybridCLR.HomologousImageMode.SuperSet))
                {
                    IsBackground = true
                };
                thread.Start();
                yield return new WaitUntil(() => !thread.IsAlive);
                OnMessageLog.Invoke($"Assembly metadata: {metadata} Pached({i + 1}/{order.Result.Metadata.Count}).");
                yield return new WaitForFixedUpdate();
            }

            OnMessageLog.Invoke($"All assemblies completly loaded. Start loading GameEntry...");
            LoadCompleted.Invoke();
        }

        private void OnEnable()
        {
            Application.logMessageReceived += Log2Screen;

        }
        private void OnDisable()
        {
            Application.logMessageReceived -= Log2Screen;
        }

        private void Log2Screen(string msg, string stackTrace, LogType type)
        {
            OnMessageLog.Invoke($"[{type}]: {msg}");
            if (type == LogType.Error)
            {
                StopAllCoroutines();
                Application.logMessageReceived -= Log2Screen;

            }
        }
    }

}