namespace Cat.Intergration.XRIT.LocomotionSystem
{
    using UnityEngine;
    using Cinemachine;
    using UnityEngine.InputSystem;

    public class VirtualPlayerCamera : CinemachineVirtualCamera
    {

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
            InternalUpdateCameraState(Vector3.up, 0);
        }

    }
}