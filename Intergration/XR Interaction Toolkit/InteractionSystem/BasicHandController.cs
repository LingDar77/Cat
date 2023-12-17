#if XRIT
using SFC.Utillities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace SFC.Intergration.XRIT.InteractionSystem
{
    public class BasicHandController : ActionBasedController
    {
        [SerializeField] private InputActionProperty HeadPositionInput;

        protected override void ApplyControllerState(XRInteractionUpdateOrder.UpdatePhase updatePhase, XRControllerState controllerState)
        {
            if (controllerState == null || !controllerState.isTracked || controllerState.position.NearlyEqualsTo(Vector3.zero, .05f)) return;
            controllerState.position -= HeadPositionInput.action.ReadValue<Vector3>();
            base.ApplyControllerState(updatePhase, controllerState);
        }
    }
}
#endif