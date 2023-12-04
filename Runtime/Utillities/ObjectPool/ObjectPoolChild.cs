using UnityEngine;

namespace SFC.Utillities
{
    public class ObjectPoolChild : MonoBehaviour, IObjectPoolChild
    {
        private GameObject prototype;
        public GameObject GetPrototype()
        {
            return prototype;
        }

        public void ResetInstance(GameObject prototype)
        {
            this.prototype = prototype;
        }

        public void Terminate()
        {
        }
    }
}
