namespace Cat.SceneManagementSystem
{
    using System.Collections;
    using UnityEngine.SceneManagement;

    public interface ISceneManagement : ICatSystem<ISceneManagement>
    {
        float LoadingProgress { get; }
        void LoadScene(string scene, LoadSceneMode loadMode = LoadSceneMode.Single);
        IEnumerator LoadSceneAsync(string scene, LoadSceneMode loadMode = LoadSceneMode.Single);

    }

}
