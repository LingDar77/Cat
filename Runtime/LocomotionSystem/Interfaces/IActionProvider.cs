using UnityEngine;

namespace TUI.LocomotioinSystem
{
    /// <summary>
    /// Basic definition of an action provider.
    /// An action provider is defined to simulate a certain movement for charactor.
    /// The action must be registerd into a locomotion system to work correctly.<see cref="ILocomotionSystem" />
    /// The system is using a certain velocity & rotation to drive the charactor.
    /// The action can participate in this process by implementing some functioins. 
    /// </summary>
    public interface IActionProvider : ITransformGetable, IEnabledSetable
    {
        /// <summary>
        /// Participate in the process of handling velocity of this charactor.
        /// </summary>
        /// <param name="currentVelocity"> The current velocity of this system. </param> 
        /// <param name="deltaTime"> The delta time to last process. </param>
        void ProcessVelocity(ref Vector3 currentVelocity, float deltaTime);
        /// <summary>
        /// Participate in the process of handling rotation of this charactor.
        /// </summary>
        /// <param name="currentRotation"> The current rotation of this system. </param> 
        /// <param name="deltaTime"> The delta time to last process. </param>
        void ProcessRotation(ref Quaternion currentRotation, float deltaTime);
        /// <summary>
        /// The pre-handled process before actual process.
        /// You can do some initialization like update new input before the actual work.
        /// </summary>
        /// <param name="deltaTime"> The delta time to last process. </param>
        void BeforeProcess(float deltaTime);
        /// <summary>
        /// The post-handled process after actual process.
        /// You can clear up or complete some special action.
        /// </summary>
        /// <param name="deltaTime"> The delta time to last process. </param>
        void AfterProcess(float deltaTime);
    }
}