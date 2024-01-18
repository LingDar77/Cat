
namespace TUI.LocomotioinSystem
{
    using System.Collections.Generic;
    using TUI.Utillities;
    using UnityEngine;
    public abstract class LocomotionSystemBase : MonoBehaviour, ILocomotionSystem
    {
        [field: SerializeField, ReadOnlyInEditor]
        public Vector3 CurrentVelocity { get; set; }
        [field: SerializeField, ReadOnlyInEditor]
        public Quaternion CurrentRotation { get; set; }
        public ICollection<IActionProvider> ActionProviders { get; set; } = new HashSet<IActionProvider>();


        public virtual void RegisterActionProvider(IActionProvider action)
        {
            ActionProviders.Add(action);
        }

        public virtual void UnregisterActionProvider(IActionProvider action)
        {
            ActionProviders.Remove(action);
        }
        public virtual bool IsStableOnGround()
        {
            return false;
        }

        public virtual void MarkUngrounded()
        {
        }

        public virtual bool IsColliderValid(Collider collider)
        {
            return true;
        }
    }
}