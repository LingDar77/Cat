#if XRIT
namespace TUI.Intergration.XRIT.InteractionSystem
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.XR.Interaction.Toolkit;
    
    public class BasicHandController : ActionBasedController
    {
        [SerializeField] private InputActionProperty HeadPositionInput;

        protected override void ApplyControllerState(XRInteractionUpdateOrder.UpdatePhase updatePhase, XRControllerState controllerState)
        {
            if (controllerState == null) return;
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                controllerState.position = positionAction.action.ReadValue<Vector3>() - HeadPositionInput.action.ReadValue<Vector3>();
            }
            base.ApplyControllerState(updatePhase, controllerState);
        }
    }
}
#endif