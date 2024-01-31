namespace TUI
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    public class InputActionsManager : MonoBehaviour
    {
        public bool EnableActionsOnStartup = true;
        public InputActionAsset[] ActionMaps;

        private void OnEnable()
        {
            if (!EnableActionsOnStartup) return;

            EnabelAllActions();
        }
        private void OnDisable()
        {
            if (!EnableActionsOnStartup) return;

            DisableAllActions();
        }

        public void EnabelAllActions()
        {
            foreach (var asset in ActionMaps)
            {
                foreach (var map in asset.actionMaps)
                {
                    map.Enable();
                }
            }
        }

        public void DisableAllActions()
        {
            foreach (var asset in ActionMaps)
            {
                foreach (var map in asset.actionMaps)
                {
                    map.Disable();
                }

            }
        }

    }
}
