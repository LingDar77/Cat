#if XRIT 
using SFC.KinematicLocomotionSystem.Actions;
using UnityEngine;

namespace SFC.Intergration.XRIT.KinematicLocomotionSystem.Actions
{
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
#endif