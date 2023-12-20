using System.Collections.Generic;
using UnityEngine;

namespace SFC.Utillities
{
    public class ConsoleToScreen : MonoBehaviour
    {
        [SerializeField] private LogType LogLevel = LogType.Log;
        [SerializeField] private Vector2 ScreenSize = new(1200, 800);
        [SerializeField] private Vector2 Position = new(100, 100);

        [SerializeField] private MonoBehaviour TraceTraget;
        const int maxLines = 50;
        const int maxLineLength = 120;
        private string _logStr = "";

        private readonly List<string> _lines = new List<string>();

        public int fontSize = 15;

        void OnEnable() { Application.logMessageReceived += Log; }
        void OnDisable() { Application.logMessageReceived -= Log; }

        public void Log(string logString, string stackTrace, LogType type)
        {
            if (type > LogLevel || (TraceTraget != null && !stackTrace.Contains(TraceTraget.GetType().Name))) return;
            foreach (var line in logString.Split('\n'))
            {
                if (line.Length <= maxLineLength)
                {
                    _lines.Add(line);
                    continue;
                }
                var lineCount = line.Length / maxLineLength + 1;
                for (int i = 0; i < lineCount; i++)
                {
                    if ((i + 1) * maxLineLength <= line.Length)
                    {
                        _lines.Add(line.Substring(i * maxLineLength, maxLineLength));
                    }
                    else
                    {
                        _lines.Add(line.Substring(i * maxLineLength, line.Length - i * maxLineLength));
                    }
                }
            }
            if (_lines.Count > maxLines)
            {
                _lines.RemoveRange(0, _lines.Count - maxLines);
            }
            _logStr = string.Join("\n", _lines);
        }

        void OnGUI()
        {
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
               new Vector3(Screen.width / ScreenSize.x, Screen.height / ScreenSize.y, 1.0f));
            GUI.Label(new Rect(Position.x, Position.y, 800, 370), _logStr, new GUIStyle() { fontSize = Mathf.Max(10, fontSize) });
        }
    }


}
