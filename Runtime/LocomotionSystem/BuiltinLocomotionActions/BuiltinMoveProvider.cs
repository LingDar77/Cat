using TUI.LocomotionSystem.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TUI
{
    public class BuiltinMoveProvider : ActionProviderBase
    {
        [SerializeField] private InputActionProperty moveControl;

        private Vector2 input;

        protected override void OnEnable()
        {
            base.OnEnable();
            moveControl.action.Enable();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            moveControl.action.Disable();
        }
        public override void BeforeProcess(float deltaTime)
        {
            input = moveControl.action.ReadValue<Vector2>();
        }
        public override void ProcessVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            currentVelocity = new(input.x, currentVelocity.y, input.y);
            if (LocomotionSystem.IsStableOnGround()) return;
            currentVelocity.y -= 9.8f * deltaTime;
        }
    }
}
