using UnityEngine;
namespace TUI.PoolifiedObjects
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