using TUI.LocomotionSystem.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TUI
{
    public class BuiltinMoveProvider : ActionProviderBase
    {
        [SerializeField] private InputActionProperty moveControl;

        private Vector2 input;
        [ContextMenu("Random Teleport")]
        private void RandomTeleport()
        {
            LocomotionSystem.SetPosition(new Vector3(Random.Range(-10f, 10f), 4, Random.Range(-10f, 10f)));
        }
        public override void BeforeProcess(float deltaTime)
        {
            input = moveControl.action.ReadValue<Vector2>();
        }

        public override void ProcessVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            currentVelocity = new(input.x, currentVelocity.y, input.y);
            if (LocomotionSystem.IsStableOnGround() && currentVelocity.y == 0) return;
            currentVelocity.y -= 9.8f * deltaTime;
        }

        public override void ProcessRotation(ref Quaternion currentRotation, float deltaTime)
        {
            // currentRotation *= Quaternion.Euler(0, 2, 0);
        }
    }
}
