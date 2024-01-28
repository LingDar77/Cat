namespace TUI.LocomotionSystem
{
    using System.Collections.Generic;
    using TUI.Utillities;
    using UnityEngine;
    public abstract class LocomotionSystemBase : MonoBehaviour, ILocomotionSystem
    {
        public Vector3 CurrentVelocity => GetCurrentVelocity();
        public Quaternion CurrentRotation => GetCurrentRotation();
        public IList<IActionProvider> ActionProviders { get; private set; } = new List<IActionProvider>();

        protected virtual Vector3 GetCurrentVelocity()
        {
            return Vector3.zero;
        }
        protected virtual Quaternion GetCurrentRotation()
        {
            return Quaternion.identity;
        }
        public virtual void RegisterActionProvider(IActionProvider action)
        {
            ActionProviders.Add(action);
        }

        public virtual void UnregisterActionProvider(IActionProvider action)
        {
            ActionProviders.Remove(action);
        }
        public virtual bool IsOnGround()
        {
            return true;
        }
        public virtual bool IsStable()
        {
            return true;
        }
        public virtual void MarkUngrounded()
        {
        }

        public virtual bool IsColliderValid(Collider collider)
        {
            return true;
        }

        public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
        }

        public virtual void SetPosition(Vector3 position)
        {
        }

        public virtual void SetRotation(Quaternion rotation)
        {
        }

        protected virtual void BeforeUpdate(float time)
        {
            for (int i = 0; i != ActionProviders.Count; ++i)
            {
                ActionProviders[i].BeforeProcess(time);
            }
        }

        protected virtual void PostUpdate(float time)
        {
            for (int i = 0; i != ActionProviders.Count; ++i)
            {
                ActionProviders[i].AfterProcess(time);
            }
        }

        protected virtual void UpdateVelocity(ref Vector3 velocity, float time)
        {
            for (int i = 0; i != ActionProviders.Count; ++i)
            {
                ActionProviders[i].ProcessVelocity(ref velocity, time);

            }
        }

        protected virtual void UpdateRotation(ref Quaternion rotation, float time)
        {
            for (int i = 0; i != ActionProviders.Count; ++i)
            {
                ActionProviders[i].ProcessRotation(ref rotation, time);

            }
        }

    }
}