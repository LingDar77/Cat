using UnityEngine;

namespace SFC
{
    public interface ITransformGetable
    {
#pragma warning disable IDE1006 // 命名样式
        Transform transform { get; }
#pragma warning restore IDE1006 // 命名样式
    }
}