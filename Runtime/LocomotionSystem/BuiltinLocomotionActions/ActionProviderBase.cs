namespace TUI.LocomotionSystem.Actions
{
    using UnityEngine;

    /// <summary>
    /// Basic implement for a action provider.
    /// Handled registeration and unregisteration, caching the locomotion system for derived actions.
    /// </summary>
    public abstract class ActionProviderBase : MonoBehaviour, IActionProvider
    {
        protected ILocomotionSystem LocomotionSystem;

        protected virtual void Awake()
        {
            LocomotionSystem ??= GetComponentInParent<ILocomotionSystem>();
        }
        protected virtual void OnEnable()
        {
            LocomotionSystem.RegisterActionProvider(this);
        }
        protected virtual void OnDisable()
        {
            LocomotionSystem.UnregisterActionProvider(this);
        }
        public virtual void AfterProcess(float deltaTime)
        {
        }

        public virtual void BeforeProcess(float deltaTime)
        {
        }

        public virtual void ProcessRotation(ref Quaternion currentRotation, float deltaTime)
        {
        }

        public virtual void ProcessVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
        }


    }
}