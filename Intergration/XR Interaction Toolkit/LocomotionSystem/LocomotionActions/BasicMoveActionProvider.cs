namespace TUI.Intergration.XRIT.KinematicLocomotionSystem.Actions
{
    using TUI.Utillities;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.InputSystem;
    using UnityEngine.XR;
    using System.Collections.Generic;
    using TUI.LocomotioinSystem;
    using TUI.LocomotionSystem.Actions;

    /// <summary>
    /// A basic move action to drive vr charctor.
    /// </summary>
    public class BasicMoveActionProvider : ActionProviderBase
    {
        [Tooltip("The Controler Stick Input.")]
        [SerializeField] private InputActionProperty ControllerMoveInput;
        [Tooltip("The HMD Velocity Input.")]
        [SerializeField] private InputActionProperty HeadMoveInput;
        [Tooltip("The Maximum Move Speed")]
        [SerializeField] private float MaxMoveSpeed = 2f;
        [Tooltip("Can the Charactor Perform Move in Air?")]
        [SerializeField] private bool CanMoveInAir = false;
        [SerializeField] private float InputMultiplierInAir = .75f;
        [ImplementedInterface(typeof(IRotateBiasable))]
        [SerializeField] private Object BiasableImplement;
        private IRotateBiasable biasable;
        public UnityEvent<float> OnVelocityUpdated;
        private Vector2 moveInputValue;
        private Vector3 headInputValue;


        /// <summary>
        /// Try apply gravity when the charactor is not stable on ground.
        /// </summary>
        /// <param name="currentVelocity"></param>
        /// <param name="deltaTime"></param>
        protected void TryApplyGravity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (LocomotionSystem.IsStableOnGround()) return;
            currentVelocity.y -= 9.8f * deltaTime;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            biasable = BiasableImplement as IRotateBiasable;
        }

        private void Start()
        {
            List<XRInputSubsystem> subsystems = new();
            SubsystemManager.GetInstances(subsystems);
            if (subsystems.Count == 0) return;
            subsystems[0].TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
        }

        public override void BeforeProcess(float deltaTime)
        {
            base.BeforeProcess(deltaTime);
            moveInputValue = ControllerMoveInput.action.ReadValue<Vector2>();
            headInputValue = biasable.Bias * HeadMoveInput.action.ReadValue<Vector3>();
            if (!LocomotionSystem.IsStableOnGround())
            {
                moveInputValue *= InputMultiplierInAir;
            }
        }

        public override void ProcessVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            base.ProcessVelocity(ref currentVelocity, deltaTime);

            if (!CanMoveInAir && !LocomotionSystem.IsStableOnGround())
            {
                TryApplyGravity(ref currentVelocity, deltaTime);
                return;
            }

            currentVelocity.x = headInputValue.x;
            currentVelocity.z = headInputValue.z;

            var referencedMoveInputValue = moveInputValue.TransformVelocityTowards(biasable.transform, transform).normalized;
            var referencedMoveStrength = moveInputValue.sqrMagnitude * MaxMoveSpeed;
            currentVelocity.x += referencedMoveInputValue.x * referencedMoveStrength;
            currentVelocity.z += referencedMoveInputValue.z * referencedMoveStrength;

            OnVelocityUpdated.Invoke(new Vector2(currentVelocity.x, currentVelocity.z).magnitude);

            TryApplyGravity(ref currentVelocity, deltaTime);
        }

    }
}