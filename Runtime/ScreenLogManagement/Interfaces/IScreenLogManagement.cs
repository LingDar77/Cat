using System.Collections.Generic;
using TUI.Library;
using UnityEngine;

namespace TUI.ScreenLogManagementSystem
{
    public interface IScreenLogManagement : ISingletonSystem<IScreenLogManagement>
    {
        List<IScreenLogFilter> Filters { get; }
        Vector2 ContentSize { get; }
        int FontSize { get; }

        void LogToScreen(LogType type, string message, string stackTrace = null);

        public static void Log(LogType type, string message, string stackTrace = null)
        {
            Singleton?.LogToScreen(type, message, stackTrace);
        }
    }
}