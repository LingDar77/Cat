#if XRIT
using TUI.KinematicLocomotionSystem;
using TUI.KinematicLocomotionSystem.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TUI.Intergration.XRIT.KinematicLocomotionSystem.Actions
{
    public class BasicSnapTurnActionProvider : ActionProviderBase
    {
        [ImplementedInterface(typeof(IRotateBiasable))]
        [SerializeField] private Object BiasableImplement;
        [Header("Inputs")]
        [SerializeField] private InputActionProperty TurnInput;
        [SerializeField] private float TurnSnapCooldownTime = .5f;
        [SerializeField] private float SnapTurnAngle = 60f;
        private float turnInputValue;
        private bool readyToTurn;
        private float timer;
        private IRotateBiasable biasable;
        private KinematicCharacterMotor motor;

        protected override void OnEnable()
        {
            base.OnEnable();
            biasable = BiasableImplement as IRotateBiasable;

            if (LocomotionSystem is KinematicLocomotionController controller)
            {
                motor = controller.Motor;
            }
        }

        public override void BeforeProcess(float deltaTime)
        {

            base.BeforeProcess(deltaTime);
            if (TurnInput.reference)
                turnInputValue = TurnInput.action.ReadValue<Vector2>().x;

        }
        public override void ProcessRotation(ref Quaternion currentRotation, float deltaTime)
        {
            base.ProcessRotation(ref currentRotation, deltaTime);
            TickSnapTurn(deltaTime);
        }

        private void TickSnapTurn(float deltaTime)
        {
            if (readyToTurn && Mathf.Abs(turnInputValue) > .8f)
            {
                var rot = Quaternion.Euler(0, SnapTurnAngle * turnInputValue, 0);
                motor.SetRotation(motor.transform.rotation * rot);
                biasable.Bias *= rot;

                timer = 0;
                readyToTurn = false;
            }
            if (!readyToTurn)
            {
                timer += deltaTime;
                if (timer >= TurnSnapCooldownTime)
                {
                    readyToTurn = true;
                }
            }
        }
    }
}
#endif