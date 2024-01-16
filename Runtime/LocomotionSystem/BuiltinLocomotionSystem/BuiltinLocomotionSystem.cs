using System.Collections;
using System.Collections.Generic;
using TUI.Utillities;
using UnityEngine;

namespace TUI.LocomotioinSystem
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class BuiltinLocomotionSystem : MonoBehaviour, ILocomotionSystem
    {
        [field: ReadOnlyInEditor]
        [field: SerializeField] public Vector3 CurrentVelocity { get; set; }
        [field: ReadOnlyInEditor]
        [field: SerializeField] public Quaternion CurrentRotation { get; set; }
        public ICollection<IActionProvider> ActionProviders { get; set; } = new HashSet<IActionProvider>();

        public void RegisterActionProvider(IActionProvider action)
        {
            ActionProviders.Add(action);
        }

        public void UnregisterActionProvider(IActionProvider action)
        {
            ActionProviders.Remove(action);
        }

        public bool IsStableOnGround()
        {
            throw new System.NotImplementedException();
        }

        public void MarkUngrounded()
        {
            throw new System.NotImplementedException();
        }
    }
}
