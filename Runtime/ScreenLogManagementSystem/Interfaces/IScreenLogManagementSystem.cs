using System.Collections.Generic;
using UnityEngine;

namespace TUI.ScreenLogManagementSystem
{
    public interface IScreenLogManagementSystem : ISingletonSystem<IScreenLogManagementSystem>
    {
        List<IScreenLogFilter> Filters { get; }
        Vector2 ContentSize { get; }
        int FontSize { get; }

    }
}