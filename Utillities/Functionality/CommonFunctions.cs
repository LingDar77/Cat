namespace Cat
{
    using UnityEngine;

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
    }
}
