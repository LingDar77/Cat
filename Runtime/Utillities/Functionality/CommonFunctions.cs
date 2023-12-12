using UnityEngine;

namespace SFC
{
    public class CommonFunctions : MonoBehaviour
    {
        public void SetLocalScale(float scale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
