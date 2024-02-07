namespace Cat.Intergration.Addressables
{
    using System.Collections;
    using Cat.SceneManagementSystem;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    using Addressables = UnityEngine.AddressableAssets.Addressables;
    public class AddressableSceneManagement : BuiltinSceneManagement
    {

        public override void LoadScene(string scene, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            Addressables.LoadSceneAsync(scene, loadMode).WaitForCompletion();
        }

        public override IEnumerator LoadSceneAsync(string scene, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            LoadingProgress = 0;
            var loadHandle = Addressables.LoadSceneAsync(scene, loadMode, false);
            while (!loadHandle.IsDone)
            {
                LoadingProgress = Mathf.Clamp(LoadingProgress + Random.Range(.1f, .5f) * Time.deltaTime, 0, .9f);
                yield return null;
            }
            var activateHandle = loadHandle.Result.ActivateAsync();
            while (!activateHandle.isDone)
            {
                LoadingProgress = activateHandle.progress;
                yield return null;
            }
            yield return new WaitForSeconds(4);
        }


    }
}
