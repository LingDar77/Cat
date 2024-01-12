using System.Collections;
using UnityEngine.SceneManagement;

namespace TUI.SceneManagementSystem
{
    public class BuiltinSceneManagement : SingletonSystemBase<BuiltinSceneManagement>, ISceneManagement
    {
        public float LoadingProgress { get; set; }

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
