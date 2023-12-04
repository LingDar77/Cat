using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SFC.SceneManagementSystem
{
    public interface ISceneManagementSystem : ISingletonSystem<ISceneManagementSystem>
    {
        void LoadScene(string scene, LoadSceneMode loadMode = LoadSceneMode.Single);
        IEnumerator LoadSceneAsync(string scene, LoadSceneMode loadMode = LoadSceneMode.Single);
    }

}
