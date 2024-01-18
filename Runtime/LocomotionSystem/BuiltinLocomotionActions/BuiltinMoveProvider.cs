using TUI.LocomotioinSystem.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TUI
{
    public class BuiltinMoveProvider : ActionProviderBase
    {
        [SerializeField] private InputActionProperty moveControl;
        [SerializeField] private InputActionProperty jumpContrl;

        private Vector2 input;

        protected override void OnEnable()
        {
            base.OnEnable();
            moveControl.action.Enable();
            jumpContrl.action.Enable();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            moveControl.action.Disable();
            jumpContrl.action.Disable();
        }
        public override void BeforeProcess(float deltaTime)
        {
            input = moveControl.action.ReadValue<Vector2>();
        }
        public override void ProcessVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            currentVelocity = new(input.x, currentVelocity.y, input.y);
            if(LocomotionSystem.IsStableOnGround() && jumpContrl.action.IsPressed())
            {
                currentVelocity.y += 5f;
                LocomotionSystem.MarkUngrounded();
            }
            currentVelocity.y -= 9.8f * deltaTime;
        }
    }
}
