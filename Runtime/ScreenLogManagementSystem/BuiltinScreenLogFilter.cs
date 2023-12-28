namespace TUI.ScreenLogManagementSystem
{
    using UnityEngine;

    [DefaultExecutionOrder(-500)]

    public class BuiltinScreenLogFilter : MonoBehaviour, IScreenLogFilter
    {
        [field: SerializeField] public LogType TracedLogLevel { get; set; } = LogType.Log;
        [field: SerializeField] public MonoBehaviour[] TracedScriptInstances { get; set; }

        protected virtual void OnEnable()
        {
            if (IScreenLogManagementSystem.Singleton == null) return;
            IScreenLogManagementSystem.Singleton.Filters.Add(this);
        }

        protected virtual void OnDisable()
        {
            if (IScreenLogManagementSystem.Singleton == null) return;
            IScreenLogManagementSystem.Singleton.Filters.Remove(this);
        }
    }
}