namespace TUI.Intergration.XRIT.LocomotionSystem.Actions
{
    using System.Collections;
    using TUI.SDKManagementSystem;
    using TUI.SDKProvider;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class BuiltinHeadRotationTracking : MonoBehaviour
    {
        [SerializeField] private Quaternion bias = Quaternion.identity;
        public Quaternion RotationBias { get => bias; set => bias = value; }
        public Transform[] SyncTransforms;
        [SerializeField] private InputActionProperty IsTrackedInput;
        [SerializeField] private InputActionProperty HeadRotationInput;
        [Header("Simulation Input")]
        [SerializeField] private InputActionProperty RotateViewInput;

        private bool initialized = false;
        public bool Initialized { get => initialized; }

        private Vector2 simulationInput;
        private IXRSDKProvider[] sdks;
        private Quaternion initial;

        private IEnumerator Start()
        {
            RotationBias = initial = transform.root.rotation;
            yield return new WaitUntil(() => IsTrackedInput.action.IsPressed());
            RotationBias = initial * Quaternion.Inverse(Quaternion.Euler(0, HeadRotationInput.action.ReadValue<Quaternion>().eulerAngles.y, 0));
            initialized = true;


            sdks = ISingletonSystem<BuiltinSDKManagement>.GetChecked().GetValidProviders<IXRSDKProvider>();
            if (sdks == null) yield break;
            foreach (var sdk in sdks)
            {
                sdk.OnRecenterSuccessed += OnRecenter;
            }
        }

        private void OnDestroy()
        {
            if (sdks == null) return;
            foreach (var sdk in sdks)
            {
                sdk.OnRecenterSuccessed -= OnRecenter;
            }
        }

        private void OnRecenter()
        {
            RotationBias = initial * Quaternion.Inverse(Quaternion.Euler(0, HeadRotationInput.action.ReadValue<Quaternion>().eulerAngles.y, 0));
        }


        private void LateUpdate()
        {

            if (Initialized)
            {
                transform.rotation = RotationBias * HeadRotationInput.action.ReadValue<Quaternion>();
                if (SyncTransforms == null) return;
                foreach (var trans in SyncTransforms)
                {
                    trans.rotation = RotationBias;
                }
                return;
            }

            var input = Vector2.zero;
            if (Mouse.current.rightButton.isPressed) input = RotateViewInput.action.ReadValue<Vector2>();

            simulationInput += input;
            simulationInput.y = Mathf.Clamp(simulationInput.y, -90, 90);
            var rot = Quaternion.Euler(-simulationInput.y, simulationInput.x, 0);
            transform.rotation = RotationBias * rot;
        }

    }
}