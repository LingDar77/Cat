using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SFC.SceneManagementSystem
{
    public class BuiltinSceneManagement : MonoBehaviour, ISceneManagementSystem, ISingletonSystem<BuiltinSceneManagement>
    {
        public float LoadingProgress { get; set; }

        protected virtual void OnEnable()
        {
            if (ISingletonSystem<BuiltinSceneManagement>.Singleton != null) return;

            ISingletonSystem<BuiltinSceneManagement>.Singleton = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }

        protected virtual void OnDisable()
        {
            if (ISingletonSystem<BuiltinSceneManagement>.Singleton.transform != this) return;
            ISingletonSystem<BuiltinSceneManagement>.Singleton = null;
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
