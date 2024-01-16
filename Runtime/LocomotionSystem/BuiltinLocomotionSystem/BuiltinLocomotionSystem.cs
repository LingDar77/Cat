using System.Collections;
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

        private void Update()
        {
            var time = Time.deltaTime;
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

            transform.SetPositionAndRotation(transform.position + currentVelocity * time, currentRotation);

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
