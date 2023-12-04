using UnityEngine;

namespace SFC.Utillities
{
    public interface IObjectPoolChild
    {
        void ResetInstance(GameObject prototype);
        GameObject GetPrototype();
        void Terminate();
    }
}
