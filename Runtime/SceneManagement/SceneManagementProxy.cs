namespace Cat.ScreenLogManagementSystem
{
    using System.Collections;
    using Cat.SceneManagementSystem;
    using Cat.Utillities;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SceneManagementProxy : MonoBehaviour, ISceneManagement
    {
        [ImplementedInterface(typeof(ISceneManagement))]
        public MonoBehaviour SceneManagementOverride;
        private ISceneManagement SceneManagement;
        public float LoadingProgress { get; }

#if UNITY_EDITOR
        public string TestScene2Load = "Test Scene";
#endif

        private void Start()
        {
            if (SceneManagementOverride == null) SceneManagement = ISingletonSystem<BuiltinSceneManagement>.GetChecked();
        }
#if UNITY_EDITOR
        [ContextMenu("Load Test Scene")]
        public void LoadScene()
        {
            SceneManagement.LoadScene(TestScene2Load, LoadSceneMode.Single);
        }
        [ContextMenu("Load Test Scene Async")]
        public void LoadSceneAsync()
        {
            SceneManagement.LoadSceneAsync(TestScene2Load, LoadSceneMode.Single);
        }
#endif
        public void LoadScene(string scene)
        {
            SceneManagement.LoadScene(scene, LoadSceneMode.Single);
        }
        public void LoadSceneAsync(string scene)
        {
            SceneManagement.LoadSceneAsync(scene, LoadSceneMode.Single);
        }

        public void LoadScene(string scene, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            SceneManagement.LoadScene(scene, loadMode);
        }

        public IEnumerator LoadSceneAsync(string scene, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            yield return SceneManagement.LoadSceneAsync(scene, loadMode);
        }
    }
}