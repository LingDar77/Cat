using UnityEngine;

namespace SFC.ScreenLogManagementSystem
{
    public interface IScreenLogFilter : IEnabledSetable
    {
        LogType TracedLogLevel { get; }
        MonoBehaviour[] TracedScriptInstances { get; }
    }
}