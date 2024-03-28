namespace Cat.ScreenLogManagementSystem
{
    using UnityEngine;
    using Cat.Utilities;

    public class BuiltinScreenLogProxy : MonoBehaviour
    {
        public void LogContent(string content)
        {
            this.Log(content);
        }

        public void Drive(float p1, float p2)
        {
            this.Log($"Value1: {p1}, Value2: {p2}");
        }
    }
}