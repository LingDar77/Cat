using UnityEngine;

namespace SFC.PoolifiedObjects
{
    public interface IGameObjectGenerator
    {
        Transform GenerateObject();
    }
}