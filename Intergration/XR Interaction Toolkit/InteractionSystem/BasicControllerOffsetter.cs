namespace TUI.Intergration.XRIT.InteractionSystem
{
    using TUI.Intergration.XRIT.LocomotionSystem.Actions;
    using UnityEngine;
    public class BasicControllerOffsetter : MonoBehaviour
    {
        [SerializeField] private BuiltinHeadRotationTracking Tracking;


        private void FixedUpdate()
        {
            LateUpdate();
        }
        private void LateUpdate()
        {
            if (!Tracking.Initialized) return;
            transform.rotation = Tracking.RotationBias;
        }
    }
}