namespace Cat
{
    using Cat.Utilities;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class CommonFunctions : MonoBehaviour
    {
        public void SetLocalScale(float scale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }

        public void SetParentNull()
        {
            transform.SetParent(null);
        }

        public void SetLocalPositionYOffset(float offset)
        {
            transform.localPosition = transform.localPosition + new Vector3(0, offset, 0);
        }

        public void MoveToScene(string sceneName)
        {
            CoroutineHelper.WaitForSeconds(() =>
            {
                SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                CoroutineHelper.Context.StopAllCoroutines();
            }, 4f);
        }
    }
}
