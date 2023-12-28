using System.Collections;
using UnityEngine.SceneManagement;

namespace TUI.SceneManagementSystem
{
    public interface ISceneManagementSystem : IGameSystem<ISceneManagementSystem>
    {
        float LoadingProgress { get; }
        void LoadScene(string scene, LoadSceneMode loadMode = LoadSceneMode.Single);
        IEnumerator LoadSceneAsync(string scene, LoadSceneMode loadMode = LoadSceneMode.Single);

    }

}
