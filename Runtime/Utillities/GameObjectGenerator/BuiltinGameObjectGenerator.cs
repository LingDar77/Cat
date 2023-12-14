using UnityEngine;
namespace SFC.Utillities
{

    public class BuiltinGameObjectGenerator : MonoBehaviour, IGameObjectGenerator
    {
        [SerializeField] protected Transform Prefab;

        public Transform GenerateObject()
        {
            return Instantiate(Prefab);
        }
    }
}