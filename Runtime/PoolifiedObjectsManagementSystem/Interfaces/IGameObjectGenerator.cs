using UnityEngine;

namespace TUI.PoolifiedObjects
{
    public interface IGameObjectGenerator
    {
        Transform GenerateObject();
    }
}