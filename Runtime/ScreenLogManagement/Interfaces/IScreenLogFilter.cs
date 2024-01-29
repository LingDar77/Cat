using UnityEngine;

namespace TUI.ScreenLogManagementSystem
{
    public interface IScreenLogFilter : IEnabledSetable
    {
        LogType TracedLogLevel { get; }
        string[] TracedScriptInstances { get; }
    }
}