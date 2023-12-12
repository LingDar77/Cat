using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SFC.SceneManagementSystem
{
    public class BuiltinSceneManagement : MonoBehaviour, ISceneManagementSystem
    {
        public float LoadingProgress { get; set; }

        public void OnEnable()
        {
            if (ISceneManagementSystem.Singleton != null) return;

            ISceneManagementSystem.Singleton = this;
            DontDestroyOnLoad(gameObject);
        }

        public void OnDisable()
        {
            if (ISceneManagementSystem.Singleton.transform != this) return;
            ISceneManagementSystem.Singleton = null;
        }

        public void LoadScene(string scene, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            SceneManager.LoadScene(scene, loadMode);
        }

        public IEnumerator LoadSceneAsync(string scene, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            LoadingProgress = 0;
            var handle = SceneManager.LoadSceneAsync(scene, loadMode);
            while (!handle.isDone)
            {
                LoadingProgress = handle.progress;
                yield return null;
            }

        }

    }
}
