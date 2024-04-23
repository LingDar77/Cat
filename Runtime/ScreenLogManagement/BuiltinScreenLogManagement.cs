namespace Cat.ScreenLogManagementSystem
{
    using System.Collections.Generic;
    using UnityEngine;

    [DefaultExecutionOrder(-1000)]
    public class BuiltinScreenLogManagement : MonoBehaviour, IScreenLogManagement
    {
        [field: SerializeField]
        public Vector2 ContentSize { get; set; } = new(.8f, .5f);
        public Vector2 Offset = Vector2.zero;

        [Range(1, 32)]
        public int MaxLines = 32;
        [field: SerializeField] public int FontSize { get; set; } = 16;
        public List<string> LogLevelColors = new() { "red", "red", "yellow", "white", "red" };
        public List<string> LogTextColors = new() { "red", "orange", "orange", "white", "red" };
        public List<string> LogTextTypes = new() { "Error", "Assert", "Warning", "Log", "Exception" };


        public List<IScreenLogFilter> Filters { get; set; } = new();

        private zstring messageToPrint;
        private readonly List<zstring> lines = new();


        protected virtual void OnEnable()
        {
            if (IScreenLogManagement.Singleton != null) return;

            IScreenLogManagement.Singleton = this;
            DontDestroyOnLoad(transform.root.gameObject);

            Application.logMessageReceived += Log2Screen;
        }

        protected virtual void OnDisable()
        {
            if (IScreenLogManagement.Singleton.transform != transform) return;
            IScreenLogManagement.Singleton = null;
            Application.logMessageReceived -= Log2Screen;

        }
        protected virtual bool FilterLogMessage(string logString, string stackTrace, LogType type)
        {
            if (logString == null) return false;
            if(Filters.Count == 0) return true;
            foreach (var filter in Filters)
            {
                if (!filter.enabled || !filter.Filter(logString, stackTrace, type)) continue;

                return true;
            }
            return false;
        }
        protected virtual zstring DecorateText(zstring text, LogType type)
        {
            return zstring.Concat("<b><color=", LogLevelColors[(int)type], ">[", LogTextTypes[(int)type], "]:</color> <color=", LogTextColors[(int)type], ">", text, "</color></b>");
        }
        public virtual void LogToScreen(LogType type, string message, string stackTrace = null)
        {
            Log2Screen(message, stackTrace, type);
        }
        protected virtual void Log2Screen(string logString, string stackTrace, LogType type)
        {
            if (!FilterLogMessage(logString, stackTrace, type)) return;

            int maxLines = (int)(720 * ContentSize.y) / FontSize;
            maxLines = Mathf.Min(maxLines, MaxLines);

            using (zstring.Block())
            {
                var current = DecorateText(logString, type);
                lines.Add(current);

                if (lines.Count > maxLines)
                {
                    lines.RemoveRange(0, lines.Count - maxLines);
                }

                messageToPrint = "";
                foreach (var line in lines)
                {
                    messageToPrint = zstring.Concat(messageToPrint, "\n", line);
                }
            }

        }


        private void OnGUI()
        {
            if (messageToPrint == null) return;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
            GUI.Label(new Rect(
                Screen.width * Offset.x,
                Screen.height * Offset.y,
                Screen.width * ContentSize.x,
                Screen.height * ContentSize.y),
                messageToPrint,
                new GUIStyle()
                {
                    fontSize = FontSize * Screen.width / 1080,
                    richText = true,
                    wordWrap = true
                });
        }


    }
}
