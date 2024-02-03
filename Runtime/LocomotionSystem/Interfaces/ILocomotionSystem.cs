namespace TUI.LocomotionSystem
{
    using UnityEngine;

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
        /// Register an action to drive this system <see cref="IActionProvider" />
        /// </summary>
        /// <param name="action"></param>
        void RegisterActionProvider(IActionProvider action);
        /// <summary>
        /// Unregister an action in this system <see cref="IActionProvider" />
        /// </summary>
        /// <param name="action"></param>
        void UnregisterActionProvider(IActionProvider action);
        bool IsStable();

        bool IsOnGround();
        /// <summary>
        /// Mark the charactor ungrounded, you must do this to clare that the charctor can leave in air.
        /// </summary>
        void MarkUngrounded();

        bool IsColliderValid(Collider collider);

        void SetPositionAndRotation(Vector3 position, Quaternion rotation);

        void SetPosition(Vector3 position);

        void SetRotation(Quaternion rotation);
        Vector3 GetGroundNormal();
    }

}
