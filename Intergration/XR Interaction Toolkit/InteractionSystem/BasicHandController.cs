using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace SFC.Intergration.XRIT.InteractionSystem
{
    public class BasicHandController : ActionBasedController
    {
        [SerializeField] private InputActionProperty HeadPositionInput;

        protected override void UpdateInput(XRControllerState controllerState)
        {
            base.UpdateInput(controllerState);
            controllerState.position -= HeadPositionInput.action.ReadValue<Vector3>();
        }
    }
}