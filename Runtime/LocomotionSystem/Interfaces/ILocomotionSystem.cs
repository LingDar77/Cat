using System.Collections.Generic;
using UnityEngine;

namespace SFC
{
    /// <summary>
    /// Basic definition of locomotion system which is used to move a charactor.
    /// The system is driven by ActionProviders <see cref="IActionProvider" />.
    /// </summary>
    public interface ILocomotionSystem : IGameSystem<ILocomotionSystem>
    {
        /// <summary>
        /// The current velocity of this system
        /// </summary>
        Vector3 CurrentVelocity { get; }
        /// <summary>
        /// The current rotation of this system
        /// </summary>
        Quaternion CurrentRotation { get; }
        /// <summary>
        /// All action providers.
        /// </summary>
        ICollection<IActionProvider> ActionProviders { get; }
        /// <summary>
        /// Register an action to drive this system <see cref="IActionProvider" />
        /// </summary>
        /// <param name="action"></param>
        void RegisterActionProvider(IActionProvider action);
        /// <summary>
        /// Unregister an action in this system <see cref="IActionProvider" />
        /// </summary>
        /// <param name="action"></param>
        void UnregisterActionProvider(IActionProvider action);

        /// <summary>
        /// Check if the charactor is stably standing on ground.
        /// </summary>
        /// <returns> Is the charactor stable on ground </returns>
        bool IsStableOnGround();
        /// <summary>
        /// Mark the charactor ungrounded, you must do this to clare that the charctor can leave in air.
        /// </summary>
        void MarkUngrounded();
    }

}
