namespace TUI.Intergration.XRIT.LocomotionSystem.Actions
{
    using TUI.LocomotionSystem.Actions;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class BuiltinSnapTurnProvider : ActionProviderBase
    {
        [SerializeField] private float TurnAmount = 45f;
        [SerializeField] private float TurnCooldownTime = .1f;
        [SerializeField] private InputActionProperty TurnAction;
        [SerializeField] private BuiltinHeadRotationTracking Tracking;
        [SerializeField] private BuiltinHeadVelocityProvider VelocityProvider;

        private float turnInput = 0;
        private bool canTurn = true;
        public override void BeforeProcess(float deltaTime)
        {
            turnInput = 0;
            if (TurnAction == null || !canTurn || Mathf.Abs(TurnAction.action.ReadValue<Vector2>().x) < .5f) return;
            turnInput = Mathf.Sign(TurnAction.action.ReadValue<Vector2>().x);
        }
        public override void ProcessRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (turnInput == 0 || !canTurn) return;
            var trans = Quaternion.Euler(0, turnInput * TurnAmount, 0);
            currentRotation *= trans;
            Tracking.RotationBias *= trans;
            VelocityProvider.VelocityBias *= trans;
            canTurn = false;
            CoroutineHelper.WaitForSeconds(ResetTimer, TurnCooldownTime);
        }
        private void ResetTimer()
        {
            canTurn = true;
        }
    }
}
