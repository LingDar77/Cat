#if XRIT
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace TUI.Intergration.XRIT.InteractionSystem
{
    public class BasicHandController : ActionBasedController
    {
        [SerializeField] private InputActionProperty HeadPositionInput;
        [SerializeField] private InputActionProperty UserPresenceInput;

        protected override void ApplyControllerState(XRInteractionUpdateOrder.UpdatePhase updatePhase, XRControllerState controllerState)
        {
            if (controllerState == null) return;
            if (UserPresenceInput.action.IsPressed() && (!controllerState.isTracked || controllerState.position == Vector3.zero)) return;
            controllerState.position -= HeadPositionInput.action.ReadValue<Vector3>();
            base.ApplyControllerState(updatePhase, controllerState);
        }
    }
}
#endif