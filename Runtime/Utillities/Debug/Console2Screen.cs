using System.Collections.Generic;
using UnityEngine;

namespace SFC.Utillities
{
    public class ConsoleToScreen : MonoBehaviour
    {
        [SerializeField] private LogType LogLevel = LogType.Log;
        [SerializeField] private Vector2Int Position = new(100, 100);

        [SerializeField] private MonoBehaviour[] TraceTragets;
        private string logMessage = "";
        private readonly List<string> lines = new();

        public int fontSize = 15;

        void OnEnable() { Application.logMessageReceived += Log; }
        void OnDisable() { Application.logMessageReceived -= Log; }

        private bool ShouldTrace(string stackTrace)
        {
            if (TraceTragets == null || TraceTragets.Length == 0) return true;
            foreach (var traget in TraceTragets)
            {
                if (stackTrace.Contains(traget.GetType().Name)) return true;
            }
            return false;
        }

        public void Log(string logString, string stackTrace, LogType type)
        {
            if (type > LogLevel || !ShouldTrace(stackTrace)) return;
            int maxLineLength = Mathf.Min(120, Screen.width / fontSize);
            int maxLines = Mathf.Min(50, Screen.height / fontSize);
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
                        lines.Add(line.Substring(i * maxLineLength, line.Length - i * maxLineLength));
                    }
                }
            }
            if (lines.Count > maxLines)
            {
                lines.RemoveRange(0, lines.Count - maxLines);
            }
            logMessage = string.Join("\n", lines);
        }

        void OnGUI()
        {
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
               Vector3.one);
            GUI.Label(new Rect(Position.x, Position.y, Screen.width - Position.x, Screen.height - Position.y), logMessage, new GUIStyle() { fontSize = Mathf.Max(10, fontSize) * Screen.height / 720 });
        }
    }


}
