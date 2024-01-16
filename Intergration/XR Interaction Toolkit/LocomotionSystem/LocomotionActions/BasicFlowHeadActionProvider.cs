namespace TUI.Intergration.XRIT.KinematicLocomotionSystem.Actions
{
    using TUI.LocomotioinSystem.Actions;
    using UnityEngine;

    public class BasicFlowHeadActionProvider : ActionProviderBase
    {
        [Tooltip("The forward reference.")]
        [SerializeField] private Transform ForwardReference;
        public override void ProcessRotation(ref Quaternion currentRotation, float deltaTime)
        {
            var target = Quaternion.Euler(0, ForwardReference.eulerAngles.y, 0);
            currentRotation = Quaternion.Lerp(currentRotation, target, deltaTime * 2);
        }
    }
}