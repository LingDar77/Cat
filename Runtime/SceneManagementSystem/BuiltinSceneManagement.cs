using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SFC.SceneManagementSystem
{
    public class BuiltinSceneManagement : MonoBehaviour, ISceneManagementSystem
    {
        private void Awake()
        {
            if (ISceneManagementSystem.Singleton != null) return;

            ISceneManagementSystem.Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        public void LoadScene(string scene, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            SceneManager.LoadScene(scene, loadMode);
        }

        public IEnumerator LoadSceneAsync(string scene, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            yield return SceneManager.LoadSceneAsync(scene, loadMode);
        }
    }
}
