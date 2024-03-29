namespace Cat.ScreenLogManagementSystem
{
    using UnityEngine;

    [DefaultExecutionOrder(-500)]

    public class BuiltinScreenLogFilter : MonoBehaviour, IScreenLogFilter
    {
        [System.Serializable]
        public enum FilterMode
        {
            WhiteList,
            BlackList
        }
        [System.Serializable]
        public class FilterRule
        {
            public FilterMode Mode = FilterMode.WhiteList;
            public LogType TracedLogLevel = LogType.Exception;
            public string[] Keywords;

            // public bool Filter(string message, string stackTrace, LogType type)
            // {
            //     if (Keywords == null || Keywords.Length == 0) return true;

            //     if (Mode == FilterMode.WhiteList)
            //     {
            //         //white list
            //         foreach (var keyword in Keywords)
            //         {
            //             if (message.Contains(keyword) || stackTrace.Contains(keyword)) return true;
            //         }
            //         return false;
            //     }
            //     else
            //     {
            //         if (type == TracedLogLevel)
            //         {
            //             if (Keywords == null || Keywords.Length == 0) return false;
            //             //black list
            //             foreach (var keyword in Keywords)
            //             {
            //                 if (message.Contains(keyword) || stackTrace.Contains(keyword)) return false;
            //             }
            //         }
            //         return true;
            //     }

            // }

            public bool Filter(string message, string stackTrace, LogType type)
            {
                if (Mode == FilterMode.BlackList)
                {
                    if (type != TracedLogLevel) return true;
                    if (Keywords == null || Keywords.Length == 0) return false;

                    foreach (var keyword in Keywords)
                    {
                        if (message.Contains(keyword) || stackTrace.Contains(keyword)) return false;
                    }
                }

                if (Mode == FilterMode.WhiteList)
                {
                    if (type != TracedLogLevel) return true;
                    if (Keywords == null || Keywords.Length == 0) return true;

                    foreach (var keyword in Keywords)
                    {
                        if (message.Contains(keyword) || stackTrace.Contains(keyword)) return true;
                    }
                    return false;
                }

                return true;
            }
        }
        public FilterRule[] rules;
        protected virtual void OnEnable()
        {
            if (IScreenLogManagement.Singleton == null) return;

            IScreenLogManagement.Singleton.Filters.Add(this);
        }

        protected virtual void OnDisable()
        {
            if (IScreenLogManagement.Singleton == null) return;
            IScreenLogManagement.Singleton.Filters.Remove(this);
        }

        public bool Filter(string message, string stackTrace, LogType type)
        {
            if (rules == null || rules.Length == 0) return true;
            var result = true;
            foreach (var rule in rules)
            {
                result &= rule.Filter(message, stackTrace, type);
                if (result == false) break;
            }
            return result;
        }
    }
}