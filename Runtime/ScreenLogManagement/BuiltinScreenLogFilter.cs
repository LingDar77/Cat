namespace TUI.ScreenLogManagementSystem
{
    using UnityEngine;

    [DefaultExecutionOrder(-500)]

    public class BuiltinScreenLogFilter : MonoBehaviour, IScreenLogFilter
    {
        [field: SerializeField] public LogType TracedLogLevel { get; set; } = LogType.Log;
        [SerializeField] private MonoBehaviour[] Traces;
        public string[] TracedScriptInstances => instanceNames;
        private string[] instanceNames;
        protected virtual void OnEnable()
        {
            if (IScreenLogManagement.Singleton == null) return;

            instanceNames = new string[Traces.Length];
            for (int i = 0; i < Traces.Length; i++)
            {
                instanceNames[i] = Traces[i].GetType().Name;
            }

            IScreenLogManagement.Singleton.Filters.Add(this);
        }

        protected virtual void OnDisable()
        {
            if (IScreenLogManagement.Singleton == null) return;
            IScreenLogManagement.Singleton.Filters.Remove(this);
        }

    }
}