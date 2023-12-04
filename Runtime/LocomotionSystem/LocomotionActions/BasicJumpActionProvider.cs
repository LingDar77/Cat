using UnityEngine;
using UnityEngine.InputSystem;

namespace SFC.KinematicLocomotionSystem.Actions
{
    /// <summary>
    /// A basic implementation of a jump action
    /// </summary>
    public class BasicJumpActionProvider : ActionProviderBase
    {
        [Tooltip("The Key to Perform Jump.")]
        [SerializeField] private InputActionProperty JumpInput;
        [Tooltip("The Velocity to Gain.")]
        [SerializeField] private float JumpVelocity = 5f;

        protected override void OnEnable()
        {
            base.OnEnable();
        }
        public override void ProcessVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            base.ProcessVelocity(ref currentVelocity, deltaTime);
            if (!LocomotionSystem.IsStableOnGround() || JumpInput.reference == null || !JumpInput.action.IsPressed()) return;

            LocomotionSystem.MarkUngrounded();
            currentVelocity.y = JumpVelocity;
        }
    }
}
