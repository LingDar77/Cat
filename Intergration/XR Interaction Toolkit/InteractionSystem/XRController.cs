namespace TUI.Intergration.XRIT.InteractionSystem
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.XR.Interaction.Toolkit;

    public class XRController : ActionBasedController
    {
        [SerializeField] protected InputActionProperty HeadPositionInput;

        protected override void UpdateTrackingInput(XRControllerState controllerState)
        {
            base.UpdateTrackingInput(controllerState);
            controllerState.position -= HeadPositionInput.action.ReadValue<Vector3>();
        }
    }
}