#if XRIT
using System.Collections;
using System.Collections.Generic;
using TUI.LocomotionSystem;
using TUI.Utillities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace TUI.Intergration.XRIT.LocomotionSystem.Actions
{
    public class BasicHeadRotationTracking : MonoBehaviour, IRotateBiasable
    {
        [SerializeField] private Quaternion bias = Quaternion.identity;
        public Quaternion Bias { get => bias; set => bias = value; }
        [SerializeField] private InputActionProperty IsTrackedInput;
        [SerializeField] private InputActionProperty HeadRotationInput;
        [Header("Simulation Input")]
        [SerializeField] private InputActionProperty RotateViewInput;

        private bool initialized = false;
        public bool Initialized { get => initialized; }

        private Vector2 simulationInput;
        private IEnumerator Start()
        {
            List<XRInputSubsystem> subsystems = new();
            SubsystemManager.GetInstances(subsystems);
            if (subsystems.Count != 0)
            {
                this.Log($"Set tracking mode: {subsystems[0].TrySetTrackingOriginMode(TrackingOriginModeFlags.Device)}");
            }
            Bias = transform.root.localRotation;
            yield return new WaitUntil(() => IsTrackedInput.action.IsPressed());
            Bias *= Quaternion.Inverse(Quaternion.Euler(0, HeadRotationInput.action.ReadValue<Quaternion>().eulerAngles.y, 0));
            initialized = true;
        }

        private void LateUpdate()
        {

            if (Initialized)
            {
                transform.rotation = Bias * HeadRotationInput.action.ReadValue<Quaternion>();
                return;
            }

            var input = Vector2.zero;
            if (Mouse.current.rightButton.isPressed) input = RotateViewInput.action.ReadValue<Vector2>();

            simulationInput += input;
            simulationInput.y = Mathf.Clamp(simulationInput.y, -90, 90);
            var rot = Quaternion.Euler(-simulationInput.y, simulationInput.x, 0);
            transform.rotation = Bias * rot;
        }

    }
}
#endif