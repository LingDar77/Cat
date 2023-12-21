using System.Collections.Generic;
using UnityEngine;

namespace SFC.ScreenLogManagementSystem
{
    public class BuiltinScreenLogManagement : MonoBehaviour, IScreenLogManagementSystem
    {
        [field: SerializeField]
        public Vector2 ContentSize { get; set; } = new(.8f, .5f);
        [field: SerializeField] public int FontSize { get; set; } = 16;
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
            if (filter.TracedScriptInstances == null || filter.TracedScriptInstances.Length == 0) return true;
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
        protected virtual void Log2Screen(string logString, string stackTrace, LogType type)
        {
            if (!FilterLogMessage(logString, stackTrace, type)) return;

            int maxLineLength = (int)(1080 * ContentSize.x) / FontSize;
            int maxLines = (int)(720 * ContentSize.y) / FontSize;
            foreach (var line in logString.Split('\n'))
            {
                if (line.Length <= maxLineLength)
                {
                    lines.Add(line);
                    continue;
                }
                var lineCount = line.Length / maxLineLength + 1;
                for (int i = 0; i < lineCount; i++)
                {
                    if ((i + 1) * maxLineLength <= line.Length)
                    {
                        lines.Add(line.Substring(i * maxLineLength, maxLineLength));
                    }
                    else
                    {
                        lines.Add(line[(i * maxLineLength)..]);
                    }
                }
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
                new GUIStyle() { fontSize = FontSize * Screen.height / 720 });
        }
    }
}
