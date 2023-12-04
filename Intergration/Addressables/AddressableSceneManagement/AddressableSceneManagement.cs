using System.Collections;
using SFC.SceneManagementSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace SFC.Intergration.AA
{
    public class AddressableSceneManagement : MonoBehaviour, ISceneManagementSystem
    {
        public void LoadScene(string scene, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            Addressables.LoadSceneAsync(scene, loadMode).WaitForCompletion();
        }

        public IEnumerator LoadSceneAsync(string scene, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            yield return Addressables.LoadSceneAsync(scene, loadMode);
        }

        private void Awake()
        {
            if (ISceneManagementSystem.Singleton != null) return;

            ISceneManagementSystem.Singleton = this;
            DontDestroyOnLoad(gameObject);


        }
    }
}
