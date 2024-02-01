#if XRIT
namespace TUI.Intergration.XRIT.LocomotionSystem.Actions
{
    using System.Collections;
    using TUI.LocomotionSystem;
    using TUI.SDKManagementSystem;
    using TUI.SDKProvider;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.InputSystem;

    public class BasicHeadRotationTracking : MonoBehaviour, IRotateBiasable
    {
        [SerializeField] private Quaternion bias = Quaternion.identity;
        public Quaternion Bias { get => bias; set => bias = value; }
        [SerializeField] private InputActionProperty IsTrackedInput;
        [SerializeField] private InputActionProperty HeadRotationInput;
        [Header("Simulation Input")]
        [SerializeField] private InputActionProperty RotateViewInput;
        public UnityEvent<Quaternion> OnInitialized;

        private bool initialized = false;
        public bool Initialized { get => initialized; }

        private Vector2 simulationInput;
        private Quaternion initialRotation;
        private IXRSDKProvider[] sdks;

        private IEnumerator Start()
        {
            Bias = transform.root.localRotation;
            initialRotation = Bias;
            yield return new WaitUntil(() => IsTrackedInput.action.IsPressed());
            Init();
            sdks = ISingletonSystem<BuiltinSDKManagement>.GetChecked().GetValidProviders<IXRSDKProvider>();
            if (sdks == null) yield break;
            foreach (var sdk in sdks)
            {
                sdk.OnRecenterSuccessed += Init;
            }
        }
        private void OnDestroy()
        {
            if (sdks == null) return;
            foreach (var sdk in sdks)
            {
                sdk.OnRecenterSuccessed -= Init;
            }
        }

        private void Init()
        {
            Bias = initialRotation * Quaternion.Inverse(Quaternion.Euler(0, HeadRotationInput.action.ReadValue<Quaternion>().eulerAngles.y, 0));
            initialized = true;
            OnInitialized.Invoke(Bias);
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