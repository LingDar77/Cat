namespace Cat.Intergration.XRIT.LocomotionSystem.Actions
{
    using Cat.LocomotionSystem.Actions;
    using Cat.Utillities;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class BuiltinSnapTurnProvider : ActionProviderBase
    {
        [SerializeField] private float TurnAmount = 45f;
        [SerializeField] private float TurnCooldownTime = .25f;
        [SerializeField] private InputActionProperty TurnAction;
        [SerializeField] private BuiltinHeadRotationTracking Tracking;
        [SerializeField] private BuiltinHeadVelocityProvider VelocityProvider;

        private float turnInput = 0;
        private bool canTurn = true;
        private bool turnFinished = false;

        public override void BeforeProcess(float deltaTime)
        {
            turnInput = 0;
            if (TurnAction == null) return;
            if (turnFinished && Mathf.Abs(TurnAction.action.ReadValue<Vector2>().x) < .1f)
            {
                turnFinished = false;
            }
            if (canTurn && !turnFinished && Mathf.Abs(TurnAction.action.ReadValue<Vector2>().x) > .8f)
            {
                turnInput = Mathf.Sign(TurnAction.action.ReadValue<Vector2>().x);
            }

        }
        
        public override void ProcessRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (turnInput == 0 || !canTurn) return;
            ApllyTurn(ref currentRotation);
            canTurn = false;
            turnFinished = true;
            CoroutineHelper.WaitForSeconds(ResetTimer, TurnCooldownTime);
        }

        private void ApllyTurn(ref Quaternion currentRotation)
        {
            var trans = Quaternion.Euler(0, turnInput * TurnAmount, 0);
            currentRotation *= trans;
            Tracking.RotationBias *= trans;
            VelocityProvider.VelocityBias *= trans;
        }

        private void ResetTimer()
        {
            canTurn = true;
        }
    }
}
