namespace Cat.ScreenLogManagementSystem
{
    using UnityEngine;
    using Cat.Utillities;

    public class BuiltinScreenLogProxy : MonoBehaviour
    {
        [ImplementedInterface(typeof(IScreenLogManagement))]
        public MonoBehaviour ScreenLogManagementOverride;
        private IScreenLogManagement ScreenLogManagement;

        private void Start()
        {
            if (ScreenLogManagementOverride == null)
            {
                ScreenLogManagement = ISingletonSystem<BuiltinScreenLogManagement>.GetChecked();
                return;
            }
            ScreenLogManagement = ScreenLogManagementOverride as BuiltinScreenLogManagement;
        }

        public void LogContent(string content)
        {
            this.Log(content);
        }

    }
}