namespace Cat.ScreenLogManagementSystem
{
    using System.Collections;
    using Cat.SceneManagementSystem;
    using Cat.Utilities;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SceneManagementProxy : MonoBehaviour
    {
        [ImplementedInterface(typeof(ISceneManagement))]
        public MonoBehaviour SceneManagementOverride;
        private ISceneManagement SceneManagement;
        public float LoadingProgress =>SceneManagement.LoadingProgress;

        private void Start()
        {
            if (SceneManagementOverride == null)
            {
                SceneManagement = ISingletonSystem<BuiltinSceneManagement>.GetChecked();
                return;
            }
            SceneManagement = SceneManagementOverride as ISceneManagement;
        }
        
        public void LoadScene(string scene)
        {
            SceneManagement.LoadScene(scene, LoadSceneMode.Single);
        }
        public void LoadSceneAsync(string scene)
        {
            CoroutineHelper.Context.StartCoroutine(SceneManagement.LoadSceneAsync(scene, LoadSceneMode.Single));
        }

    }
}