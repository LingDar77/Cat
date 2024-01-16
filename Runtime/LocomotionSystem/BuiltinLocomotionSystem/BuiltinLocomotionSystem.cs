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
        [SerializeField] protected CapsuleCollider capsule;

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
            var halfHight = capsule.height / 2;
            var point1 = transform.position + Vector3.up * halfHight;
            var point2 = transform.position - Vector3.up * halfHight;

            var speedY = currentVelocity.y;
            if (Physics.Raycast(point2, Vector3.down, speedY * time))
            {
                Debug.Log("hit ground!");
                currentVelocity.y = 0;
            }

            // var hits = Physics.CapsuleCastAll(point1, point2, capsule.radius, currentVelocity.normalized, currentVelocity.magnitude * time);
            // if (hits.Length > 1)
            // {
            //     Debug.Log("hits");
            //     currentVelocity = Vector3.zero;
            // }
            transform.SetPositionAndRotation(transform.position + currentVelocity * time, currentRotation);
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
