namespace TUI.Intergration.XRIT.LocomotionSystem.Actions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TUI.LocomotionSystem;
    using TUI.LocomotionSystem.Actions;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class BuiltinSnapTurnProvider : ActionProviderBase
    {
        [SerializeField] private InputActionProperty TurnAction;
        [SerializeField] private MonoBehaviour Tracking;

        private IRotateBiasable biasable => Tracking as IRotateBiasable;
        protected override void OnEnable()
        {
            base.OnEnable();
            TurnAction.action.performed += OnTurn;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            TurnAction.action.performed -= OnTurn;
        }

        private void OnTurn(InputAction.CallbackContext context)
        {
            biasable.RotationBias *= Quaternion.Euler(0, 45, 0);
        }
    }
}
