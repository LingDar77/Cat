using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SFC.SceneManagementSystem
{
    public class BuiltinSceneManagement : MonoBehaviour, ISceneManagementSystem
    {
        public float LoadingProgress { get; set; }

        public virtual void OnEnable()
        {
            if (ISceneManagementSystem.Singleton != null) return;

            ISceneManagementSystem.Singleton = this;
            DontDestroyOnLoad(gameObject);
        }

        public virtual void OnDisable()
        {
            if (ISceneManagementSystem.Singleton.transform != this) return;
            ISceneManagementSystem.Singleton = null;
        }

        public virtual void LoadScene(string scene, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            SceneManager.LoadScene(scene, loadMode);
        }

        public virtual IEnumerator LoadSceneAsync(string scene, LoadSceneMode loadMode = LoadSceneMode.Single)
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
