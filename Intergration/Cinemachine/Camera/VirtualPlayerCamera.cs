namespace Cat.Intergration.XRIT.LocomotionSystem
{
    using UnityEngine;
    using Cinemachine;
    using UnityEngine.InputSystem;

    public class VirtualPlayerCamera : CinemachineVirtualCameraBase
    {
        protected CameraState state = CameraState.Default;
        public override CameraState State => state;
        public override Transform LookAt { get; set; }
        public override Transform Follow { get; set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            InputSystem.onAfterUpdate += OnUpdate;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            InputSystem.onAfterUpdate -= OnUpdate;

        }

        private void OnUpdate()
        {
            state.RawPosition = transform.position;
            state.RawOrientation = transform.rotation;
        }

        public override void InternalUpdateCameraState(Vector3 worldUp, float deltaTime)
        {
            state.RawPosition = transform.position;
            state.RawOrientation = transform.rotation;
        }
    }
}