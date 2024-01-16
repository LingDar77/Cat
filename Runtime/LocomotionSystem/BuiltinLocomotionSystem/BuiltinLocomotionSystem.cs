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
        [SerializeField] private CapsuleCollider capsule;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (capsule == null) capsule = GetComponent<CapsuleCollider>();
#endif
        }

        private void Update()
        {
            var time = Time.deltaTime;

            PreparePhase(time);

            SimulatePhase(time);

            PostPhase(time);
        }


        private void PreparePhase(float time)
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

        private void SimulatePhase(float time)
        {
            var halfHight = capsule.height / 2;
            var point1 = transform.position + Vector3.up * halfHight;
            var point2 = transform.position - Vector3.up * halfHight;
            if (Physics.CapsuleCast(point1, point2, capsule.radius, currentVelocity))
            {
                Debug.Log("hit!");
            }

            transform.SetPositionAndRotation(transform.position + currentVelocity * time, currentRotation);
        }

        private void PostPhase(float time)
        {
            foreach (var provider in ActionProviders)
            {
                provider.AfterProcess(time);
            }
        }

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
