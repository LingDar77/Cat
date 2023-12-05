using System.Collections;
using SFC.SceneManagementSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace SFC.Intergration.AA
{
    public class AddressableSceneManagement : MonoBehaviour, ISceneManagementSystem
    {
        public float LoadingProgress { get; set; }

        private void Awake()
        {
            if (ISceneManagementSystem.Singleton != null) return;

            ISceneManagementSystem.Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        public void LoadScene(string scene, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            Addressables.LoadSceneAsync(scene, loadMode).WaitForCompletion();
        }

        public IEnumerator LoadSceneAsync(string scene, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            LoadingProgress = 0;
            var loadHandle = Addressables.LoadSceneAsync(scene, loadMode, false);
            yield return loadHandle;
            var activateHandle = loadHandle.Result.ActivateAsync();
            while (!activateHandle.isDone)
            {
                LoadingProgress = activateHandle.progress;
                yield return null;
            }
        }


    }
}
