namespace Cat.ScreenLogManagementSystem
{
    using UnityEngine;
    public interface IScreenLogFilter : IEnabledSetable
    {
        bool Filter(string message, string stackTrace, LogType type);
    }
}