using System.Collections.Generic;
using UnityEngine;

namespace TUI.ScreenLogManagementSystem
{
    [DefaultExecutionOrder(-1000)]
    public class BuiltinScreenLogManagement : MonoBehaviour, IScreenLogManagementSystem
    {
        [field: SerializeField]
        public Vector2 ContentSize { get; set; } = new(.8f, .5f);
        [field: SerializeField] public int FontSize { get; set; } = 16;
        public List<string> LogLevelColors = new() { "red", "red", "yellow", "white", "red" };
        public List<string> LogTextColors = new() { "red", "orange", "orange", "white", "orange" };

        public List<IScreenLogFilter> Filters { get; set; } = new();

        private string logMessage = "";
        private readonly List<string> lines = new();


        protected virtual void OnEnable()
        {
            if (IScreenLogManagementSystem.Singleton != null) return;

            IScreenLogManagementSystem.Singleton = this;
            DontDestroyOnLoad(transform.root.gameObject);

            Application.logMessageReceived += Log2Screen;
        }

        protected virtual void OnDisable()
        {
            if (IScreenLogManagementSystem.Singleton.transform != transform) return;
            IScreenLogManagementSystem.Singleton = null;
            Application.logMessageReceived -= Log2Screen;

        }
        protected virtual bool ShouldTraceForFilter(string stackTrace, IScreenLogFilter filter)
        {
            if (stackTrace == null || filter.TracedScriptInstances == null || filter.TracedScriptInstances.Length == 0) return true;
            foreach (var traget in filter.TracedScriptInstances)
            {
                if (stackTrace.Contains(traget.GetType().Name)) return true;
            }
            return false;
        }
        protected virtual bool FilterLogMessage(string logString, string stackTrace, LogType type)
        {
            foreach (var filter in Filters)
            {
                if (!filter.enabled) continue;
                if (type > filter.TracedLogLevel) continue;
                if (!ShouldTraceForFilter(stackTrace, filter)) continue;

                return true;
            }
            return false;
        }
        protected virtual string DecorateText(string text, LogType type)
        {
            return $"<b><color={LogLevelColors[(int)type]}>[{type}]:</color> <color={LogTextColors[(int)type]}>{text}</color></b>";
        }
        public virtual void LogToScreen(LogType type, string message, string stackTrace = null)
        {
            Log2Screen(message, stackTrace, type);
        }
        protected virtual void Log2Screen(string logString, string stackTrace, LogType type)
        {
            if (!FilterLogMessage(logString, stackTrace, type)) return;

            int maxLines = (int)(720 * ContentSize.y) / FontSize;

            foreach (var line in logString.Split('\n'))
            {
                var current = DecorateText(line, type);
                lines.Add(current);
            }

            if (lines.Count > maxLines)
            {
                lines.RemoveRange(0, lines.Count - maxLines);
            }
            logMessage = string.Join("\n", lines);

        }


        private void OnGUI()
        {
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
               Vector3.one);
            GUI.Label(new Rect(
                (1 - ContentSize.x) * Screen.width / 2,
                (1 - ContentSize.y) * Screen.height / 2,
                Screen.width * ContentSize.x,
                Screen.height * ContentSize.y),
                logMessage,
                new GUIStyle() { fontSize = FontSize * Screen.width / 1080, richText = true, wordWrap = true });
        }


    }
}
