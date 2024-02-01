using UnityEngine;

namespace TUI.ScreenLogManagementSystem
{
    public interface IScreenLogFilter : IEnabledSetable
    {
        bool Filter(string message, string stackTrace, LogType type);   
    }
}