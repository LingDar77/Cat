using System.Collections;
using UnityEngine.SceneManagement;

namespace SFC.SceneManagementSystem
{
    public interface ISceneManagementSystem : ISingletonSystem<ISceneManagementSystem>
    {
        float LoadingProgress { get; }
        void LoadScene(string scene, LoadSceneMode loadMode = LoadSceneMode.Single);
        IEnumerator LoadSceneAsync(string scene, LoadSceneMode loadMode = LoadSceneMode.Single);
    }

}
