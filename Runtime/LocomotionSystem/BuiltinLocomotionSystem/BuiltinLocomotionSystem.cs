using System.Collections.Generic;
using TUI.Utillities;
using UnityEngine;

namespace TUI.LocomotioinSystem
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class BuiltinLocomotionSystem : MonoBehaviour, ILocomotionSystem
    {
        public Vector3 CurrentVelocity { get => currentVelocity; }
        public Quaternion CurrentRotation { get => currentRotation; }
        public ICollection<IActionProvider> ActionProviders { get; set; } = new HashSet<IActionProvider>();
        [ReadOnlyInEditor]
        [SerializeField] private Vector3 currentVelocity;
        [ReadOnlyInEditor]
        [SerializeField] private Quaternion currentRotation;
        [ReadOnlyInEditor]
        [SerializeField] protected bool IsOnGround;
        [ReadOnlyInEditor]
        [SerializeField] protected CapsuleCollider capsule;
        [SerializeField] private LayerMask layerMask = 1;
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (capsule == null) capsule = GetComponent<CapsuleCollider>();
#endif
        }

        protected virtual void Update()
        {
            var time = Time.deltaTime;

            PreparePhase(time);

            SimulatePhase(time);

            PostPhase(time);
        }


        protected virtual void PreparePhase(float time)
        {
            foreach (var provider in ActionProviders)
            {
                provider.BeforeProcess(time);
            }

            foreach (var provider in ActionProviders)
            {
                provider.ProcessRotation(ref currentRotation, time);
            }

            foreach (var provider in ActionProviders)
            {
                provider.ProcessVelocity(ref currentVelocity, time);
            }
        }

        protected virtual void SimulatePhase(float time)
        {
          
        }

        protected virtual void PostPhase(float time)
        {
            foreach (var provider in ActionProviders)
            {
                provider.AfterProcess(time);
            }
        }

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
            throw new System.NotImplementedException();
        }

        public virtual void MarkUngrounded()
        {
            throw new System.NotImplementedException();
        }
    }
}
